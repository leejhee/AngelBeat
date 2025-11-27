using Core.Scripts.Data;
using Core.Scripts.Foundation.Define;
using Core.Scripts.Managers;
using Cysharp.Threading.Tasks;
using GamePlay.Common.Scripts.Entities.Character;
using GamePlay.Common.Scripts.Novel;
using GamePlay.Features.Battle.Scripts.BattleAction;
using GamePlay.Features.Battle.Scripts.BattleTurn;
using GamePlay.Features.Battle.Scripts.Unit;
using GamePlay.Features.Battle.Scripts.Unit.Components.AI;
using GamePlay.Features.Explore.Scripts;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace GamePlay.Features.Battle.Scripts.Tutorial
{
    public class BattleTutorialDirector : MonoBehaviour
    {
        #region Singleton
        public static BattleTutorialDirector Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        #endregion

        [Header("전투 튜토리얼에 대한 config 삽입")] 
        [SerializeField] private List<BattleTutorialConfig> configs;
        [SerializeField, ReadOnly] private BattleTutorialConfig currentConfig;
        [SerializeField] private BattleInputGate inputGate;
        
        private readonly HashSet<string> _completedSteps = new();

        private int _currentIndex;
        private TurnEventContext _lastTurnContext;
        private TurnController _turnController;
        private BattleController _battleController;
        
        public void Init(TurnController turnController, BattleController battleController)
        {
            if (configs == null || configs.Count == 0)
            {
                Debug.LogError("[BattleTutorialDirector] No config assigned.");
                return;
            }
            
            if (!BattleSession.Instance.NeedTutorial)
            {
                Destroy(gameObject);
                return;
            }
            
            _currentIndex = BattleSession.Instance.CurrentTutorialIndex;
            currentConfig = configs[_currentIndex];
            
            _turnController = turnController;
            _battleController = battleController;
            
            _battleController.OnBattleStartAsync += OnBattleStartAsync;
            _battleController.OnBattleEndAsync += OnBattleEndAsync;
            _battleController.ActionCompleted += OnActionCompleted;
            _battleController.OnActionPreviewStarted += OnActionPreviewStarted;
            
            _turnController.OnRoundProceedAsync += OnRoundProceedAsync;
            _turnController.OnTurnBeganAsync += OnTurnBeganAsync;
            _turnController.OnTurnEndedAsync += OnTurnEndedAsync;
            
        }
        
        #region Events

        private async UniTask OnBattleStartAsync()
        {
            if (currentConfig == null) return;

            foreach (var step in currentConfig.steps)
            {
                if (step.triggerType != TutorialTriggerEventType.BattleStart)
                    continue;
                if (!IsStepAvailable(step))
                    continue;

                await ExecuteStep(step);
            }
        }

        private async UniTask OnBattleEndAsync(SystemEnum.eCharType winnerType)
        {
            if (!currentConfig) return;

            foreach (var step in currentConfig.steps)
            {
                if (step.triggerType != TutorialTriggerEventType.BattleEnd)
                    continue;
                if (!IsStepAvailable(step))
                    continue;

                // 나중에 승패 조건 넣고 싶으면 여기에서 winnerType 보고 필터
                if (step.winnerType != winnerType)
                    continue;
                if (step.partyAddIndex != 0)
                {
                    CompanionData data = DataManager.Instance.GetData<CompanionData>(step.partyAddIndex);
                    if (data == null) continue;
                    CharacterModel model = new(data);
                    BattleController.Instance.PlayerParty.AddMember(model);
                }
                await ExecuteStep(step);
            }

            await HandleTutorialEndBattleAsync(winnerType);
            if (winnerType == SystemEnum.eCharType.Player && BattleSession.Instance != null)
            {
                BattleSession.Instance.MarkTutorialBattleCompleted();
            }
        }

        private async UniTask HandleTutorialEndBattleAsync(SystemEnum.eCharType winnerType)
        {
            var battle = BattleSession.Instance;
            var explore = ExploreSession.Instance;

            if (!battle.IsEndExploreBattle)
                return;
            if (winnerType != SystemEnum.eCharType.Player)
                return;
            if (_currentIndex != configs.Count - 1) // 마지막이어야지
                return;
            
            SystemEnum.Dungeon nextDungeon = SystemEnum.Dungeon.MOUNTAIN_BACK;
            int nextFloor = 1;
            Party nextParty = battle.PlayerParty; 
            explore.SetNewExplore(nextDungeon, nextFloor, nextParty);
        }
        
        private async UniTask OnRoundProceedAsync(RoundEventContext ctx)
        {
            foreach (BattleTutorialStep step in currentConfig.steps)
            {
                if (step.triggerType != TutorialTriggerEventType.RoundStart)
                    continue;
                if (!IsStepAvailable(step))
                    continue;

                // 라운드 조건
                if (step.requiredRound > 0 && step.requiredRound != ctx.Round)
                    continue;

                await ExecuteStep(step);
            }
        }

        private async UniTask OnTurnBeganAsync(TurnEventContext ctx)
        {
            _lastTurnContext = ctx;

            foreach (BattleTutorialStep step in currentConfig.steps)
            {
                if (step.triggerType != TutorialTriggerEventType.TurnStart)
                    continue;
                if (!IsStepAvailable(step))
                    continue;

                if (!MatchTurnCondition(step, ctx))
                    continue;
                
                ApplyEnemyScriptForStep(step, ctx);
                
                await ExecuteStep(step);
            }
        }

        private async UniTask OnTurnEndedAsync(TurnEventContext ctx)
        {
            foreach (BattleTutorialStep step in currentConfig.steps)
            {
                if (step.triggerType != TutorialTriggerEventType.TurnEnd)
                    continue;
                if (!IsStepAvailable(step))
                    continue;

                if (!MatchTurnCondition(step, ctx))
                    continue;

                await ExecuteStep(step);
            }
            
            ClearInputLock();
        }
        
        private void OnActionCompleted(BattleActionBase action, BattleActionResult result)
        {
            HandleActionCompletedAsync(action, result).Forget();
        }
        
        private async UniTask HandleActionCompletedAsync(BattleActionBase action, BattleActionResult result)
        {
            ClearInputLock();
            
            if (!result.ActionSuccess)
                return;
            
            TurnEventContext ctx = _lastTurnContext;

            foreach (var step in currentConfig.steps)
            {
                if (step.triggerType != TutorialTriggerEventType.ActionCompleted)
                    continue;
                if (!IsStepAvailable(step))
                    continue;

                if (!MatchTurnCondition(step, ctx))
                    continue;
                if (!MatchActionCondition(step, action, result))
                    continue;

                await ExecuteStep(step);
            }
        }
        
        private void OnActionPreviewStarted(BattleActionContext ctx, BattleActionPreviewData data)
        {
            HandleActionPreviewStartedAsync(ctx, data).Forget();
        }

        private async UniTask HandleActionPreviewStartedAsync(
            BattleActionContext ctx, 
            BattleActionPreviewData data)
        {
            TurnEventContext turnCtx = _lastTurnContext;

            foreach (BattleTutorialStep step in currentConfig.steps)
            {
                if (step.triggerType != TutorialTriggerEventType.ActionPreviewStart)
                    continue;
                if (!IsStepAvailable(step))
                    continue;

                if (!MatchTurnCondition(step, turnCtx))
                    continue;

                if (step.filterActionType && step.requiredActionType != ctx.battleActionType)
                    continue;

                // 필요하면 data(PossibleCells 등)도 여기서 활용 가능

                await ExecuteStep(step);
            }
        }
        
        #endregion
        
        #region Core Methods

        private bool SetNextTutorialConfig()
        {
            if (++_currentIndex >= configs.Count)
            {
                currentConfig = null;
                Debug.Log("튜토리얼 종료. 해당 director 개입 종료");
                return false;
            }
            currentConfig = configs[_currentIndex];
            _completedSteps.Clear();
            _lastTurnContext = null;
            return true;
        }
        
        private bool IsStepAvailable(BattleTutorialStep step)
        {
            if (!step.triggerOnce) return true;
            if (string.IsNullOrEmpty(step.stepID)) return true;

            return !_completedSteps.Contains(step.stepID);
        }

        private void MarkStepCompleted(BattleTutorialStep step)
        {
            if (!step.triggerOnce) return;
            if (string.IsNullOrEmpty(step.stepID)) return;

            _completedSteps.Add(step.stepID);
        }
        
        private void ApplyInputLockForStep(BattleTutorialStep step)
        {
            if (!inputGate || !step.lockInputDuringStep)
                return;

            inputGate.ApplyTutorialLock(step, _lastTurnContext);
        }
        
        private void ClearInputLock()
        {
            inputGate?.ClearTutorialLock();
        }
        
        private async UniTask ExecuteStep(BattleTutorialStep step)
        {
            if (!step) return;

            MarkStepCompleted(step);
            ApplyInputLockForStep(step);
            
            switch (step.viewType)
            {
                case BattleTutorialViewType.Novel:
                    await PlayNovelStepAsync(step);
                    break;
        
                case BattleTutorialViewType.Guide:
                    PlayGuideStepAsync(step).Forget();
                    break;
            }
        }
        
        private async UniTask PlayNovelStepAsync(BattleTutorialStep step)
        {
            if (string.IsNullOrEmpty(step.novelScriptId))
                return;

            await NovelDomainPlayer.PlayNovelScript(step.novelScriptId);
        }
        
        private async UniTask PlayGuideStepAsync(BattleTutorialStep step)
        {
            if (step.guidePages == null || step.guidePages.Length == 0)
                return;
    
            BattleTutorialGuideUI ui = BattleTutorialGuideUI.Instance;
            if (!ui)
            {
                Debug.LogWarning("BattleTutorialGuideUI 인스턴스가 없습니다.");
                return;
            }

            CharBase actor = _lastTurnContext?.Actor;

            foreach (BattleTutorialGuidePage page in step.guidePages)
            {
                if (page == null) continue;

                switch (page.anchor)
                {
                    case GuideAnchor.ScreenTop:
                        ui.ShowScreenTop(page.text);
                        break;
                    case GuideAnchor.Actor:
                        if (actor)
                            ui.ShowForActor(actor, page.text);
                        else
                            ui.ShowScreenTop(page.text);
                        break;
                    case GuideAnchor.ScreenPosition:
                        ui.ShowForScreenPosition(page.screenNormalizedPos, page.text);
                        break;
                }

                await ui.WaitForNextAsync();
            }

            ui.Hide();

        }
        
        private bool MatchTurnCondition(BattleTutorialStep step, TurnEventContext ctx)
        {
            if (step.requiredRound > 0 && step.requiredRound != ctx.Round)
                return false;

            CharBase actor = ctx.Actor;
            if (!actor)
                return false;

            if (step.onlyPlayer && actor.GetCharType() != SystemEnum.eCharType.Player)
                return false;
            if (step.onlyEnemy && actor.GetCharType() == SystemEnum.eCharType.Player)
                return false;

            if (step.requiredCharId != 0 && actor.GetID() != step.requiredCharId)
                return false;

            //if (step.requiredActorTurnCount > 0 && step.requiredActorTurnCount != ctx.ActorTurnCount)
            //    return false;

            return true;
        }

        private bool MatchActionCondition(BattleTutorialStep step, BattleActionBase action, BattleActionResult result)
        {
            if (step.filterActionType && action.ActionType != step.requiredActionType)
                return false;

            // 필요하면 스킬 ID 같은 조건도 여기서 추가
            return true;
        }
        
        private void ApplyEnemyScriptForStep(BattleTutorialStep step, TurnEventContext ctx)
        {
            if (!step.forceEnemyScript)
                return;

            // 현재 턴 액터
            CharMonster monster = ctx.Actor as CharMonster;
            if (!monster)
                return;

            if (step.enemyCommands == null || step.enemyCommands.Length == 0)
                return;

            List<EnemyScriptCommand> commands = new(step.enemyCommands);

            // 튜토리얼 AI 생성 후 주입
            CharTutorialAI tutorialAI = new(monster, commands);
            monster.OverrideAI(tutorialAI);

            Debug.Log($"[Tutorial] {monster.name} 에 튜토리얼 AI 스크립트 주입 (stepID: {step.stepID})");
        }
        
        #endregion
        
        private void OnDestroy()
        {
            if (_turnController != null)
            {
                _turnController.OnRoundProceedAsync -= OnRoundProceedAsync;
                _turnController.OnTurnBeganAsync -= OnTurnBeganAsync;
                _turnController.OnTurnEndedAsync -= OnTurnEndedAsync;
            }

            if (_battleController != null)
            {
                _battleController.ActionCompleted -= OnActionCompleted;
                _battleController.OnBattleStartAsync -= OnBattleStartAsync;
                _battleController.OnBattleEndAsync -= OnBattleEndAsync;
                _battleController.OnActionPreviewStarted -= OnActionPreviewStarted;
            }
            
            if (Instance == this)
                Instance = null;
        }
        
        
    }
}