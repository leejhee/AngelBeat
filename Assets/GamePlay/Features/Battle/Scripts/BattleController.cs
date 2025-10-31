using Core.Scripts.Data;
using Core.Scripts.Foundation.Define;
using Core.Scripts.Foundation.SceneUtil;
using Core.Scripts.Managers;
using Cysharp.Threading.Tasks;
using GamePlay.Common.Scripts.Entities.Character;
using GamePlay.Common.Scripts.Entities.Skills;
using GamePlay.Common.Scripts.Skill;
using GamePlay.Features.Battle.Scripts.BattleAction;
using GamePlay.Features.Battle.Scripts.BattleMap;
using GamePlay.Features.Battle.Scripts.BattleTurn;
using GamePlay.Features.Battle.Scripts.Unit;
using System;
using System.Collections.Generic;
using System.Threading;
using UIs.Runtime;
using Unity.VisualScripting;
using UnityEngine;

namespace GamePlay.Features.Battle.Scripts
{
    public class BattleController : MonoBehaviour
    {
        #region singleton
        private static BattleController instance;
        
        public static BattleController Instance
        {
            get
            {
                GameObject go = GameObject.Find("BattleController");
                if (!go)
                {
                    go = new GameObject("BattleController");
                    instance = go.AddComponent<BattleController>();
                }
                return instance;
            }
            private set => instance = value;
        }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);

            GameManager.Instance.GameState = SystemEnum.GameState.Battle;
        }
        #endregion
        
        #region UI Member

        [SerializeField] private GameObject backgroundPrefab;
        [SerializeField] private GameObject gameOverPrefab;
        [SerializeField] private GameObject gameWinPrefab;
        [SerializeField] private GameObject previewPrefab;
        #endregion
        
        #region Battle Map DataBase
        [SerializeField] private BattleFieldDB battleFieldDB;
        [SerializeField] private SystemEnum.Dungeon DebugDungeon;
        [SerializeField] private string DebugMapName;
        #endregion
        
        #region Core Field & Property
        [SerializeField] private float cameraSize = 11;
        
        private IBattleStageSource _stageSource;
        private IMapLoader _mapLoader;
        private StageField _battleStage;
        
        private TurnController _turnManager;
        public CharBase FocusChar => _turnManager.TurnOwner;
        public Party PlayerParty =>  _stageSource.PlayerParty;
        
        public event Action<long> OnCharacterDead;
        #endregion
        
        #region UI Model
        
        public class TurnStructureModel
        {
            public IReadOnlyCollection<Turn> TurnCollection;
            
            public TurnStructureModel(IReadOnlyCollection<Turn> turnCollection) =>  TurnCollection = turnCollection;
        }
        public TurnStructureModel GetChangedTurnStructureModel => new(_turnManager.TurnCollection);
        
        //TODO : temporary. 절대 그냥 냅두지 말것.
        public StageField GetStageField()
        {
            return _battleStage;
        }
        
        public BattleStageGrid GetBattleGrid()
        {
            return _battleStage?.GetComponent<BattleStageGrid>();
        }
        
        public TurnController TurnController => _turnManager;
        
        #endregion
        private async void Start()
        {
            Debug.Log("Starting Battle...");
            if (_stageSource == null)
            {
                Party party = new();
                party.InitPartyAsync();
                BattlePayload.Instance.SetBattleData(party, DebugDungeon, DebugMapName);
                
                Debug.Log("Stage source not set : Using Battle Payload");
                _stageSource = new BattlePayloadSource();
            }
            _mapLoader = new StageLoader(_stageSource, battleFieldDB);
            await BattleInitialize();
            
            await _turnManager.ChangeTurn();
        }
        
        /// <summary> 테스트 용도로 stage source를 관리체에 제공한다. </summary>
        /// <param name="stageSource"> 테스트 용도의 stage source. </param>
        public void SetStageSource(IBattleStageSource stageSource) => _stageSource = stageSource;
        
        /// <summary>
        /// 역할 : 전투 진입 시의 최초 동작 메서드. 전투 환경을 초기화한다.
        /// </summary>
        private async UniTask BattleInitialize()
        {
            Debug.Log("Starting Battle Initialization...");

            string stageName = _stageSource.StageName;
            Party playerParty = _stageSource.PlayerParty;
            
            // 맵 띄우기
            _battleStage = await _mapLoader.InstantiateBattleFieldAsync(stageName);
            BattleStageGrid stageGrid = _battleStage.GetComponent<BattleStageGrid>() 
                                        ?? _battleStage.AddComponent<BattleStageGrid>();
            stageGrid.InitGrid(_battleStage);
            // 이미지 띄우기
            
            // 맵에다가 파티를 포함시켜서 모든 애들 띄우기
            await _battleStage.SpawnAllUnits(playerParty);
            stageGrid.RebuildCharacterPositions(); //다 띄웠으면 캐릭터 위치도 전부 기록해주기
            
            // 턴 관리기 초기화
            _turnManager = new TurnController(); 
            
            // Battle 전용 UI 초기화
            await UIManager.Instance.ShowViewAsync(ViewID.BattleSceneView);
            
            Debug.Log("Battle Initialization Complete");
            BattlePayload.Instance.Clear();
            
            //카메라 초기화
            Camera.main.orthographicSize = cameraSize;
            
        }
        
        #region Battle Action Managing

        private enum BattleActionState {Idle, Preview, Execute}
        private BattleActionState _currentActionState;
        private BattleActionBase _currentActionBase;
        private BattleActionContext _currentActionContext;
        private CancellationTokenSource _actionCts;
        
        
        #region Action Indicator Settings
        [SerializeField] private GameObject indicatorPrefab;
        [SerializeField] private List<GameObject> indicatorLists = new();
        [SerializeField] private Color possibleColor;
        [SerializeField] private Color blockedColor;
        #endregion
        
        public bool IsModal => _currentActionState != BattleActionState.Idle;
        public event Action<BattleActionBase, BattleActionResult> ActionCompleted;
        
        /// <summary>
        /// BattleAction 시작을 위한 Preview 제시 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="skillSlotIndex"> 몇번 슬롯 눌렀는지 </param>
        public async UniTask StartPreview(ActionType type, int skillSlotIndex=-1)
        {
            if (_currentActionState != BattleActionState.Idle)
            {
                // 기본 상태에서만 선택 가능
                Debug.LogWarning($"[BattleController] Preview Cannot Start in state {_currentActionState}.");
                return;
            } 
            
            // 턴 행동 가능 여부를 검증
            Turn currentTurn = _turnManager.CurrentTurn;
            if (currentTurn == null)
            {
                Debug.LogWarning("[BattleController] 현재 턴이 없습니다.");
                return;
            }
            
            TurnActionState.ActionCategory category = type.GetActionCategory();
            
            // 이동이 아닌 경우
            if (category == TurnActionState.ActionCategory.MajorAction)
            {
                if (!currentTurn.CanPerformAction(category))
                {
                    Debug.LogWarning($"[BattleController] 이번 턴에는 더 이상 주요 행동(밀기/점프/스킬)을 사용할 수 없습니다.");
                    return;
                }
            }
            else
            { 
                // 이동인 경우
                if (!currentTurn.CanPerformAction(category))
                {
                    Debug.Log("[BattleController] 이번 턴에는 더 이상 이동을 할 수 없습니다.");
                    AudioClip clip = await ResourceManager.Instance.LoadAsync<AudioClip>("MoveBlocked");
                    SoundManager.Instance.Play(clip);
                    await FocusChar.BlinkSpriteOnce();
                    return;
                }
            }
            
            CancelPreview(); // 깔끔하게 남아있는 필드 초기화
            _currentActionContext = new BattleActionContext
            {
                battleActionType = type, 
                actor = FocusChar, 
                battleField = _battleStage, 
                skillModel = skillSlotIndex == -1 ? 
                    null : FocusChar.SkillInfo.SkillSlots[skillSlotIndex]
            };
            
            // Action을 만들어주고 state를 옮겨준다.
            _currentActionBase = BattleActionFactory.CreateBattleAction(_currentActionContext);
            _actionCts = new CancellationTokenSource();

            try
            {
                BattleActionPreviewData data = await _currentActionBase.BuildActionPreview(_actionCts.Token);
                RenderPreview(data);    
                _currentActionState = BattleActionState.Preview;
            }
            catch (OperationCanceledException) { /*Silence*/ }
            catch (Exception e)
            {
                Debug.LogException(e);
                CancelPreview();
            }

        }
        
        /// <summary>
        /// Preview를 취소하고 없애는 메서드
        /// </summary>
        public void CancelPreview()
        {
            if (_actionCts != null)
            {
                _actionCts.Cancel();
                _actionCts.Dispose();
                _actionCts = null;
            }
            
            HideBattleActionPreview();
            _currentActionBase = null;
            _currentActionContext = null;
            _currentActionState = BattleActionState.Idle;
        }

        public void HideBattleActionPreview()
        {
            foreach (GameObject go in indicatorLists)
            {
                Destroy(go);
            }
            indicatorLists.Clear();
            _battleStage.ShowGridOverlay(false);
        }
        
        private void RenderPreview(BattleActionPreviewData data)
        {
            HideBattleActionPreview(); //혹시 비활성화되어있거나 남아있는 거 제거
            _battleStage.ShowGridOverlay(true);
            
            foreach (var c in data.PossibleCells) CreateIndicator(c, blocked:false);
            foreach (var c in data.BlockedCells)  CreateIndicator(c, blocked:true);
        }

        private void CreateIndicator(Vector2Int cell, bool blocked)
        {
            Vector3 pos = _battleStage.CellToWorldCenter(cell);
            GameObject go = Instantiate(indicatorPrefab, pos, Quaternion.identity, _battleStage.transform);
            indicatorLists.Add(go);
            go.transform.localScale = new Vector3(_battleStage.Grid.cellSize.x, _battleStage.Grid.cellSize.y, 1f);
            
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr) sr.color = blocked ? blockedColor : possibleColor;
            
            var indi = go.GetComponent<BattleActionIndicator>();
            if (!indi) return;
            
            // 행동마다 콜백이 달라서 이런 형태로 사용
            if (_currentActionContext.battleActionType == ActionType.Skill)
            {
                indi.Init(
                    caster: FocusChar,
                    skill:  _currentActionContext.skillModel,
                    isBlocked: blocked,
                    pivotType: _currentActionContext.skillModel.skillRange.skillPivot,
                    cell: cell,
                    confirmAction: OnSkillIndicatorConfirm
                );
            }
            else
            {
                indi.InitForSimpleCell(
                    isBlocked: blocked,
                    cell: cell,
                    onClickCell: OnCellClicked
                );
            }
        }

        private async void OnSkillIndicatorConfirm(
            CharBase caster, 
            SkillModel skill, 
            List<CharBase> targets, 
            Vector2Int cell)
        {
            if (_currentActionState != BattleActionState.Preview) return;
            if (_currentActionBase == null || _currentActionContext == null) return;
            
            Turn currentTurn = _turnManager.CurrentTurn;
            if (!currentTurn.TryUseMajorAction())
            {
                Debug.LogWarning("스킬 사용 실패: 이미 주요 행동을 사용했습니다.");
                CancelPreview();
                return;
            }
            
            _currentActionContext.TargetCell = cell;
            _currentActionContext.targets =  targets;

            _currentActionState = BattleActionState.Execute;
            
            BattleActionBase finishedAction = _currentActionBase;
            BattleActionResult result = BattleActionResult.Fail(BattleActionResult.ResultReason.BattleActionAborted);
            try
            {
                HideBattleActionPreview();
                result = await _currentActionBase.ExecuteAction(_actionCts?.Token ?? CancellationToken.None);
            }
            catch (OperationCanceledException)
            { /*Silence*/ }
            catch (Exception ex) { Debug.LogException(ex); }
            finally
            {
                CancelPreview();
                ActionCompleted?.Invoke(finishedAction, result);
            }
        }

        private async void OnCellClicked(Vector2Int cell)
        {
            if (_currentActionState != BattleActionState.Preview) return;
            if (_currentActionBase == null || _currentActionContext == null) return;
            
            Turn currentTurn = _turnManager.CurrentTurn;
            
            // 이동 행동 시 이동력 검증 및 소모
            if (_currentActionContext.battleActionType == ActionType.Move)
            {
                BattleStageGrid g = _currentActionContext.battleField.GetComponent<BattleStageGrid>();
                Vector2Int startCell = g.WorldToCell(_currentActionContext.actor.transform.position);
                int moveDistance = Math.Abs(cell.x - startCell.x);
                // 이동 가능 여부
                if (!currentTurn.CanPerformAction(TurnActionState.ActionCategory.Move, moveDistance))
                {
                    Debug.LogWarning($"이동력 부족: {moveDistance:F1} 필요, {currentTurn.ActionState.RemainingMovePoint:F1} 남음");
                    CancelPreview();
                    return;
                }
                
                // 이동력 소모
                if (!currentTurn.TryConsumeMove(moveDistance))
                {
                    Debug.LogWarning("이동력 소모 실패");
                    CancelPreview();
                    return;
                }
            }
            else if (_currentActionContext.battleActionType == ActionType.Jump ||
                     _currentActionContext.battleActionType == ActionType.Push)
            {
                if (!currentTurn.TryUseMajorAction())
                {
                    Debug.LogWarning("주요 행동 사용 실패");
                    CancelPreview();
                    return;
                }
            }
            
            _currentActionContext.TargetCell = cell;
            _currentActionState = BattleActionState.Execute;
            
            BattleActionBase finishedAction = _currentActionBase;
            BattleActionResult result = BattleActionResult.Fail(BattleActionResult.ResultReason.BattleActionAborted);

            try
            {
                HideBattleActionPreview();
                result = await _currentActionBase.ExecuteAction(_actionCts?.Token ?? CancellationToken.None);
            }
            catch (OperationCanceledException)
            { /* Silence */ }
            catch (Exception ex) { Debug.LogException(ex); }
            finally
            {
                CancelPreview();
                ActionCompleted?.Invoke(finishedAction, result);
            }
        }
        
        #endregion

        public void HandleUnitDeath(CharBase unit)
        {
            if(IsModal && _currentActionContext?.actor == unit)
                CancelPreview(); // preview 중에 죽을 일은 없겠지만, 예외처리 용도
            
            // 전투 그리드 적용
            BattleStageGrid grid = _battleStage?.GetComponent<BattleStageGrid>();
            grid?.RemoveUnit(unit);
            
            // 턴 관리도 필요함
            OnCharacterDead?.Invoke(unit.GetID());
        }
        
        public async void EndBattle(SystemEnum.eCharType winnerType)
        {
            // 결과 내보내기(onBattleEnd 필요)
            if (winnerType == SystemEnum.eCharType.Player)
            {
                // 이겼을 때 보수를 주는 UI를 올린다.
                await UIManager.Instance.ShowViewAsync(ViewID.GameWinView);
            }
            else
            {
                await UIManager.Instance.ShowViewAsync(ViewID.GameOverView);
            }
            
            // 탐사로 비동기 로딩. 
            //SceneLoader.LoadSceneWithLoading(SystemEnum.eScene.ExploreScene);
        }
#if UNITY_EDITOR
        [ContextMenu("전투 승리 치트")]
        public void GameWinCheat()
        {
            EndBattle(SystemEnum.eCharType.Player);
        }
#endif
        
        
        #region UI

        public Action<CharacterModel> battleCharInfoEvent;
        
        public async void ShowCharacterInfoView()
        {
            await UIManager.Instance.ShowViewAsync(ViewID.CharacterInfoPopUpView);
            
            battleCharInfoEvent?.Invoke(FocusChar.CharInfo);
        }
        
        public Action<long> rewardSkillSelectedEvent;
        public void SelectSkillReward(long skillID)
        {
            rewardSkillSelectedEvent?.Invoke(skillID);
        }

        public void GetSkill(long skillID)
        {
            PlayerParty.AddSkillInCharacter(skillID);
        }
        
        #endregion
    }
    
}


