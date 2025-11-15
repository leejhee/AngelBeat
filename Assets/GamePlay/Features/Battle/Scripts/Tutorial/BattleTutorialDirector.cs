using Core.Scripts.Foundation.Define;
using Cysharp.Threading.Tasks;
using GamePlay.Features.Battle.Scripts.BattleAction;
using GamePlay.Features.Battle.Scripts.BattleTurn;
using GamePlay.Features.Battle.Scripts.Unit;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay.Features.Battle.Scripts.Tutorial
{
    public class BattleTutorialDirector : MonoBehaviour
    {
        [Header("전투 튜토리얼에 대한 config 삽입")]
        [SerializeField] private BattleTutorialConfig config;

        private readonly HashSet<string> _completedSteps = new();

        private TurnEventContext _lastTurnContext;

        private TurnController _turnController;
        private BattleController _battleController;
        
        public void Init(TurnController turnController, BattleController battleController)
        {
            if (config == null)
            {
                Debug.LogError("[BattleTutorialDirector] No config assigned.");
                return;
            }
            
            _turnController = turnController;
            _battleController = battleController;
            
            _battleController.OnBattleStartAsync += OnBattleStartAsync;
            _battleController.OnBattleEndAsync += OnBattleEndAsync;
            _battleController.ActionCompleted += OnActionCompleted;
            
            _turnController.OnRoundProceedAsync += OnRoundProceedAsync;
            _turnController.OnTurnBeganAsync += OnTurnBeganAsync;
            _turnController.OnTurnEndedAsync += OnTurnEndedAsync;
            
        }
        
        #region Events

        private async UniTask OnBattleStartAsync()
        {
            if (config == null) return;

            foreach (var step in config.steps)
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
            if (!config) return;

            foreach (var step in config.steps)
            {
                if (step.triggerType != TutorialTriggerEventType.BattleEnd)
                    continue;
                if (!IsStepAvailable(step))
                    continue;

                // 나중에 승패 조건 넣고 싶으면 여기에서 winnerType 보고 필터
                await ExecuteStep(step);
            }
        }
        
        private async UniTask OnRoundProceedAsync(RoundEventContext ctx)
        {
            foreach (BattleTutorialStep step in config.steps)
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

            foreach (BattleTutorialStep step in config.steps)
            {
                if (step.triggerType != TutorialTriggerEventType.TurnStart)
                    continue;
                if (!IsStepAvailable(step))
                    continue;

                if (!MatchTurnCondition(step, ctx))
                    continue;

                await ExecuteStep(step);
            }
        }

        private async UniTask OnTurnEndedAsync(TurnEventContext ctx)
        {
            foreach (BattleTutorialStep step in config.steps)
            {
                if (step.triggerType != TutorialTriggerEventType.TurnEnd)
                    continue;
                if (!IsStepAvailable(step))
                    continue;

                if (!MatchTurnCondition(step, ctx))
                    continue;

                await ExecuteStep(step);
            }
        }
        
        private void OnActionCompleted(BattleActionBase action, BattleActionResult result)
        {
            HandleActionCompletedAsync(action, result).Forget();
        }
        
        private async UniTask HandleActionCompletedAsync(BattleActionBase action, BattleActionResult result)
        {
            if (!result.ActionSuccess)
                return;

            var ctx = _lastTurnContext; // 여기서 Round/Actor/ActorTurnCount 다 쓸 수 있음

            foreach (var step in config.steps)
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
        
        #endregion
        
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

        private async UniTask ExecuteStep(BattleTutorialStep step)
        {
            MarkStepCompleted(step);
            
            // 삽입하고 싶은 이벤트가 있다면 여기에 삽입할 것.
            
            if (!string.IsNullOrEmpty(step.novelScriptId))
            {
                Debug.Log($"[TutorialDirector] 데이터에 따라, {step.stepID} - {step.novelScriptId}의 재생");
                await NovelManager.PlayScriptAndWait(step.novelScriptId);
            }
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

            if (step.requiredActorTurnCount > 0 && step.requiredActorTurnCount != ctx.ActorTurnCount)
                return false;

            return true;
        }

        private bool MatchActionCondition(BattleTutorialStep step, BattleActionBase action, BattleActionResult result)
        {
            if (step.filterActionType && action.ActionType != step.requiredActionType)
                return false;

            // 필요하면 스킬 ID 같은 조건도 여기서 추가
            return true;
        }
        
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
            }
        }
        
    }
}