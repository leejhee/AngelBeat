using Core.Scripts.Foundation.Define;
using Core.Scripts.Foundation.SceneUtil;
using Core.Scripts.Managers;
using Cysharp.Threading.Tasks;
using GamePlay.Common.Scripts.Contracts;
using GamePlay.Common.Scripts.Entities.Character;
using GamePlay.Common.Scripts.Entities.Skills;
using GamePlay.Features.Battle.Scripts.BattleAction;
using GamePlay.Features.Battle.Scripts.BattleMap;
using GamePlay.Features.Battle.Scripts.BattleTurn;
using GamePlay.Features.Battle.Scripts.Unit;
using System;
using System.Collections.Generic;
using System.Threading;
using UIs.Runtime;
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

        }
        #endregion
        
        #region Battle Map DataBase
        [SerializeField] private BattleFieldDB battleFieldDB;
        [SerializeField] private SystemEnum.Dungeon DebugDungeon;
        [SerializeField] private string DebugMapName;
        #endregion
        
        #region Core Field & Property
        [SerializeField] private float cameraSize = 11;
        [SerializeField] private BattleCameraDriver cameraDriver;
        
        private IBattleSceneSource _sceneSource;
        private IMapLoader _mapLoader;
        
        private StageField _battleStage;
        private Party _playerParty;
        private bool _initialized;
        private TurnController _turnManager;
        
        public BattleCameraDriver CameraDriver => cameraDriver;
        public CharBase FocusChar => _turnManager.TurnOwner;
        public Party PlayerParty => _playerParty;
        public TurnController TurnController => _turnManager;
        public StageField StageField => _battleStage;
        public BattleStageGrid StageGrid => _battleStage?.GetComponent<BattleStageGrid>();

        public event Func<UniTask> OnBattleStartAsync;
        public event Func<SystemEnum.eCharType, UniTask> OnBattleEndAsync;
        public event Action<BattleActionContext, BattleActionPreviewData> OnActionPreviewStarted;
        public event Action<BattleActionBase, BattleActionResult> ActionCompleted;
        public event Action<long> OnCharacterDead;
        
        #endregion
        
        public void SetStageSource(IBattleSceneSource sceneSource) => _sceneSource = sceneSource;

        public void Initialize(StageField stage, TurnController turnManager, Party party)
        {
            _battleStage = stage;
            _turnManager = turnManager;
            _playerParty = party;

            _initialized = true;
            if(Camera.main != null)
                Camera.main.orthographicSize = cameraSize;
            
            _turnManager.OnTurnBeganAsync += async m =>
            {
                //if (cameraDriver && m?.Turn?.TurnOwner)
                //    await cameraDriver.Focus(m.Turn.TurnOwner.CharCameraPos, 0.4f);
                if (cameraDriver && m.Actor)
                    await cameraDriver.Focus(m.Actor.CharCameraPos, 0.4f);
            };
        }
        
        public async UniTask RaiseBattleStartAsync()
        {
            if (OnBattleStartAsync != null)
                await OnBattleStartAsync.Invoke();
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
            
            #region 턴 행동 가능 여부 계산 
            
            Turn currentTurn = _turnManager.CurrentTurn;
            if (currentTurn == null)
            {
                Debug.LogWarning("[BattleController] 현재 턴이 없습니다.");
                return;
            }
            
            TurnActionState.ActionCategory category = type.GetActionCategory();
            
            // 이동이 아닌 경우
            if (category == TurnActionState.ActionCategory.SkillAction)
            {
                if (!currentTurn.CanPerformAction(category))
                {
                    Debug.LogWarning($"[BattleController] 이번 턴에는 더 이상 스킬을 사용할 수 없습니다.");
                    return;
                }
            }
            else if (category == TurnActionState.ActionCategory.ExtraAction)
            {
                if (!currentTurn.CanPerformAction(category))
                {
                    Debug.LogWarning($"[BattleController] 이번 턴에는 더 이상 부가행동을 사용할 수 없습니다.");
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
            
            #endregion
            
            #region 턴 행동을 위한 베이스 생성
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
            #endregion
            
            #region 행동 베이스에 따른 프리뷰 렌더링
            try
            {
                BattleActionPreviewData data = await _currentActionBase.BuildActionPreview(_actionCts.Token);
                RenderPreview(data); // 미리보기를 렌더링한다.
                _currentActionState = BattleActionState.Preview;
                OnActionPreviewStarted?.Invoke(_currentActionContext, data);
            }
            catch (OperationCanceledException) { /*Silence*/ }
            catch (Exception e)
            {
                Debug.LogException(e);
                CancelPreview();
            }
            #endregion
            
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
                    pivotType: _currentActionContext.skillModel.SkillRange.skillPivot,
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
            List<IDamageable> targets, 
            Vector2Int cell)
        {
            if (_currentActionState != BattleActionState.Preview) return;
            if (_currentActionBase == null || _currentActionContext == null) return;
            
            Turn currentTurn = _turnManager.CurrentTurn;
            if (!currentTurn.TryUseSkill())
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
                if (!currentTurn.TryUseExtra())
                {
                    Debug.LogWarning($"{_currentActionContext.battleActionType} 사용 실패");
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
            Turn t = _turnManager.FindTurn(unit);
            if (t != null)
            {
                t.KillTurn();
                // 현재 턴이 죽었으면 다음 턴으로 즉시 진행
                if (_turnManager.CurrentTurn == t)
                    _ = _turnManager.ChangeTurn();
            }

            OnCharacterDead?.Invoke(unit.GetID());
        }
        
        public async void EndBattle(SystemEnum.eCharType winnerType)
        {
            BattleCharManager.Instance.ClearAll();
            
            // 전투 종료 시의 특정 이벤트 수행
            if (OnBattleEndAsync != null)
                await OnBattleEndAsync.Invoke(winnerType);
            
            // 마지막에는 전투 승리, 패배에 따른 통상 시퀀스
            if (winnerType == SystemEnum.eCharType.Player)
            {
                await UIManager.Instance.ShowViewAsync(ViewID.GameWinView);
            }
            else
            {
                SceneLoader.LoadSceneWithLoading(SystemEnum.eScene.LobbyScene);
            }
        }
#if UNITY_EDITOR
        [ContextMenu("전투 승리 치트")]
        public void GameWinCheat()
        {
            EndBattle(SystemEnum.eCharType.Player);
        }
#endif
        
        #region UI

        public Action<CharacterModel> BattleCharInfoEvent;
        
        public async void ShowCharacterInfoView()
        {
            await UIManager.Instance.ShowViewAsync(ViewID.CharacterInfoPopUpView);
            
            BattleCharInfoEvent?.Invoke(FocusChar.CharInfo);
        }
        
        public void GetSkill(long skillID)
        {
            PlayerParty.AddSkillInCharacter(skillID);
        }
        
        #endregion
    }
    
}


