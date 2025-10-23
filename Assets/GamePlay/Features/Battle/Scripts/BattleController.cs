using Core.Scripts.Data;
using Core.Scripts.Foundation.Define;
using Core.Scripts.Foundation.SceneUtil;
using Core.Scripts.Managers;
using Cysharp.Threading.Tasks;
using GamePlay.Common.Scripts.Entities.Character;
using GamePlay.Common.Scripts.Entities.Skills;
using GamePlay.Common.Scripts.Skill;
using GamePlay.Common.Scripts.Skill.Preview;
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
    // 잠시 싱글턴 사용한다.
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
        [SerializeField] private GameObject gameOverPrefab;
        [SerializeField] private GameObject gameWinPrefab;
        [SerializeField] private GameObject previewPrefab;
        #endregion
        
        #region Battle Map DataBase
        [SerializeField] private BattleFieldDB battleFieldDB;
        [SerializeField] private SystemEnum.Dungeon DebugDungeon;
        [SerializeField] private string DebugMapName;
        #endregion
        
        [SerializeField] private float cameraSize = 11;
        
        private IBattleStageSource _stageSource;
        private IMapLoader _mapLoader;
        private StageField _battleStage;
        
        private TurnController _turnManager;
        public CharBase FocusChar => _turnManager.TurnOwner;
        public IReadOnlyList<CharacterModel> PartyList => _stageSource.PlayerParty.partyMembers;
        
        
        #region UI Model
        
        public class TurnStructureModel
        {
            public IReadOnlyCollection<Turn> TurnCollection;
            
            public TurnStructureModel(IReadOnlyCollection<Turn> turnCollection) =>  TurnCollection = turnCollection;
        }
        public TurnStructureModel GetChangedTurnStructureModel => new(_turnManager.TurnCollection);
        
        //temporary
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
            await UIManager.Instance.ShowViewAsync(ViewID.BattleSceneView);
            _turnManager.OnRoundProceeds.Invoke();
            
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
            
            // 맵에다가 파티를 포함시켜서 모든 애들 띄우기
            List<CharBase> battleMembers = await _battleStage.SpawnAllUnits(playerParty);
            stageGrid.RebuildCharacterPositions(); //다 띄웠으면 캐릭터 위치도 전부 기록해주기
            
            // 턴 관리기 초기화
            _turnManager = new TurnController(battleMembers); 
            _turnManager.ChangeTurn();
            
            
            Debug.Log("Battle Initialization Complete");
            BattlePayload.Instance.Clear();
            
            //카메라 초기화
            Camera.main.orthographicSize = cameraSize;
            

        }
        
        #region Battle Action Managing
        
        public enum BattleActionState {Idle, Preview, Execute}
        private BattleActionState _currentActionState;
        private BattleActionBase _currentActionBase;
        private BattleActionContext _currentActionContext;

        private CancellationTokenSource _actionCts;
        //private ActionType _currentActionType;
        //private SkillModel _currentSkill;
        
        #region Action Indicator Settings
        [SerializeField] private GameObject indicatorPrefab;
        [SerializeField] private List<GameObject> indicatorLists = new();
        [SerializeField] private Color possibleColor;
        [SerializeField] private Color blockedColor;
        #endregion
        
        public bool IsModal => _currentActionState != BattleActionState.Idle;
        
        /// <summary>
        /// BattleAction 시작을 위한 Preview 제시 및 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="skillSlotIndex"> 몇번 슬롯 눌렀는지 </param>
        public async UniTask StartPreview(ActionType type, int skillSlotIndex=-1)
        {
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
            //_currentSkill = null;
            _currentActionState = BattleActionState.Idle;
            //_currentActionType = ActionType.None;
        }
        
        /// <summary>
        /// 스킬 버튼 토글 시 행동 상태 조작 메서드
        /// </summary>
        /// <param name="target">입력을 제어할 스킬 모델</param>
        [Obsolete]
        public void ToggleSkillPreview(int targetIdx)
        {
            if (IsModal)
            {
                CancelPreview();
            }
            else
            {
                StartPreview(ActionType.Skill, targetIdx)
                    .AttachExternalCancellation(this.GetCancellationTokenOnDestroy())
                    .Forget(); // 여기 무조건 조심할 것 일단 fire-forget패턴으로 사용
            }
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

            _currentActionContext.TargetCell = cell;
            _currentActionContext.targets =  targets;

            _currentActionState = BattleActionState.Execute;
            try
            {
                await _currentActionBase.ExecuteAction(_actionCts?.Token ?? CancellationToken.None);
            }
            catch (OperationCanceledException) {/*Silence*/}
            catch (Exception ex) { Debug.LogException(ex); }
            finally { CancelPreview(); }
        }

        private async void OnCellClicked(Vector2Int cell)
        {
            if (_currentActionState != BattleActionState.Preview) return;
            if (_currentActionBase == null || _currentActionContext == null) return;
            
            _currentActionContext.TargetCell = cell;

            _currentActionState = BattleActionState.Execute;
            try
            {
                await _currentActionBase.ExecuteAction(_actionCts?.Token ?? CancellationToken.None);
            }
            catch (OperationCanceledException) { /* Silence */ }
            catch (Exception ex) { Debug.LogException(ex); }
            finally { CancelPreview(); }
        }
        
        #endregion
        
        public void EndBattle(SystemEnum.eCharType winnerType)
        {
            // 결과 내보내기(onBattleEnd 필요)
            if (winnerType == SystemEnum.eCharType.Player)
            {
                // 이겼을 때 보수를 주는 UI를 올린다.
                //UIManager.Instance.ShowUI(gameWinPrefab);
            }
            else
            {
                //UIManager.Instance.ShowUI(gameOverPrefab);
            }
			// 캐릭터 모델 갱신
            
            // 탐사로 비동기 로딩. 
            SceneLoader.LoadSceneWithLoading(SystemEnum.eScene.ExploreScene);
        }
        
    }
}


