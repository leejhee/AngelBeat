using Cysharp.Threading.Tasks;
using GamePlay.Features.Battle.Scripts;
using GamePlay.Features.Battle.Scripts.BattleAction;
using GamePlay.Features.Battle.Scripts.BattleMap;
using GamePlay.Features.Battle.Scripts.BattleTurn;
using GamePlay.Features.Battle.Scripts.Unit;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace GamePlay.Common.Scripts.Entities.Character.Components.AI
{
    /// <summary>
    /// 적 AI 제어 (ActionSet 기반)
    /// PDF "적 AI 판단 로직 (Simple Ver.)" 구현
    /// </summary>
    public class CharAI
    {
        private CharBase _owner;
        private AIContext _context;
        private AIActionSetGenerator _setGenerator;
        private Turn _currentTurn;
        private BattleStageGrid _grid;
        private StageField _stageField;
        
        public CharAI(CharBase owner)
        {
            _owner = owner;
        }
        
        /// <summary>
        /// 턴 시작 시 AI 실행
        /// PDF 전체 플로우 구현
        /// </summary>
        public async UniTask ExecuteTurn(Turn turn)
        {
            _currentTurn = turn;
            await UniTask.Delay(1000); // 연출을 위한 딜레이
            
            // BattleController에서 그리드 가져오기
            #region Grid Initialization
            _stageField = BattleController.Instance.GetStageField();
            if (!_stageField)
            {
                Debug.LogError("[AI] StageField를 찾을 수 없습니다.");
                return;
            }
            
            _grid = _stageField.GetComponent<BattleStageGrid>();
            if (!_grid)
            {
                Debug.LogError("[AI] BattleStageGrid를 찾을 수 없습니다.");
                return;
            }
            #endregion
            
            Debug.Log($"[AI] ====== {_owner.name} 턴 시작 ======");
            
            _context = new AIContext(_owner, _grid);
            _context.AnalyzeSituation();
            Debug.Log(_context.GetSummary());
            
            _setGenerator = new AIActionSetGenerator(_context);
            List<AIActionSet> allSets = _setGenerator.GenerateAllActionSets();
            
            foreach (var set in allSets)
            {
                _setGenerator.CheckAfterMoveForSet(set);
            }
            
            List<AIActionSet> validSets = _setGenerator.FilterInvalidSets(allSets);
            Debug.Log($"[AI] 유효한 세트: {validSets.Count}/{allSets.Count}");
            
            foreach (var set in validSets)
            {
                _setGenerator.CalculateWeight(set);
            }
            
            List<AIActionSet> topSets = _setGenerator.SelectTopSets(validSets, 3);
            
            bool actionSuccess = false;
            foreach (var set in topSets)
            {
                Debug.Log($"[AI] 시도: {set}");
                actionSuccess = await TryExecuteActionSet(set);
                
                if (actionSuccess)
                {
                    Debug.Log($"[AI] 성공: {set}");
                    break;
                }
                
                Debug.Log("[AI] 실패, 다음 후보 시도");
            }
            
            if (!actionSuccess)
            {
                Debug.LogWarning($"[AI] {_owner.name} 모든 행동 실패, 턴 종료");
            }
            
            // 턴 종료 대기 (연출용)
            await UniTask.Delay(500);
            
            Debug.Log($"[AI] ====== {_owner.name} 턴 종료 ======");
        }
        
        /// <summary>
        /// ActionSet 실행 시도
        /// </summary>
        private async UniTask<bool> TryExecuteActionSet(AIActionSet set)
        {
            try
            {
                // 1. 이동 (MoveTo)
                if (set.MoveTo.HasValue)
                {
                    bool moveSuccess = await ExecuteMove(set.MoveTo.Value);
                    if (!moveSuccess)
                    {
                        Debug.LogWarning("[AI] 초기 이동 실패");
                        return false;
                    }
                }
                
                // 2. 행동 실행
                bool actionSuccess = false;
                switch (set.AIActionType)
                {
                    case AIActionType.Attack:
                        actionSuccess = await ExecuteAttack(set);
                        break;
                    
                    case AIActionType.Push:
                        actionSuccess = await ExecutePush(set);
                        break;
                    
                    case AIActionType.Jump:
                        actionSuccess = await ExecuteJump(set);
                        break;
                    
                    case AIActionType.Move:
                        // 이미 이동 완료
                        actionSuccess = true;
                        break;
                    
                    case AIActionType.Wait:
                        await UniTask.Delay(300);
                        actionSuccess = true;
                        break;
                }
                
                if (!actionSuccess)
                {
                    Debug.LogWarning("[AI] 행동 실행 실패");
                    return false;
                }
                
                // 3. 재이동 (AfterMove)
                if (set.AfterMove.HasValue)
                {
                    bool afterMoveSuccess = await ExecuteMove(set.AfterMove.Value);
                    if (!afterMoveSuccess)
                    {
                        Debug.LogWarning("[AI] 재이동 실패 (행동은 성공)");
                        // 재이동 실패는 허용
                    }
                }
                
                return true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }
        }
        
        /// <summary>
        /// AIActionType을 BattleActionContext의 ActionType으로 변환
        /// </summary>
        private ActionType ConvertToBattleActionType(AIActionType aiActionType)
        {
            switch (aiActionType)
            {
                case AIActionType.Attack:
                    return ActionType.Skill;  // AI의 Attack은 Skill 사용
                case AIActionType.Push:
                    return ActionType.Push;
                case AIActionType.Jump:
                    return ActionType.Jump;
                case AIActionType.Move:
                    return ActionType.Move;
                case AIActionType.Wait:
                    return ActionType.None;
                default:
                    return ActionType.None;
            }
        }
        
        #region 행동 실행 메서드들
        
        /// <summary>
        /// 이동 실행
        /// </summary>
        private async UniTask<bool> ExecuteMove(Vector2Int target)
        {
            Vector2Int currentPos = _grid.WorldToCell(_owner.CharTransform.position);
            int moveDistance = Mathf.Abs(target.x - currentPos.x);
            
            // 이동력 검증
            if (!_currentTurn.CanPerformAction(TurnActionState.ActionCategory.Move, moveDistance))
            {
                Debug.LogWarning("[AI] 이동력 부족");
                return false;
            }
            
            // BattleActionContext 생성 - 올바른 ActionType 사용
            var actionContext = new BattleActionContext
            {
                battleActionType = ActionType.Move,  // BattleAction의 ActionType
                actor = _owner,
                battleField = _stageField,
                TargetCell = target
            };
            
            // MoveBattleAction 실행
            var moveAction = new MoveBattleAction(actionContext);
            var result = await moveAction.ExecuteAction(CancellationToken.None);
            
            if (result.ActionSuccess)
            {
                _currentTurn.TryConsumeMove(moveDistance);
                Debug.Log($"[AI] 이동 성공: {currentPos} → {target}");
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// 공격 실행
        /// </summary>
        private async UniTask<bool> ExecuteAttack(AIActionSet set)
        {
            if (!_currentTurn.CanPerformAction(TurnActionState.ActionCategory.MajorAction))
            {
                Debug.LogWarning("[AI] 주요 행동 사용 불가");
                return false;
            }
            
            if (set.SkillToUse == null || !set.TargetChar|| !set.TargetCell.HasValue)
            {
                Debug.LogWarning("[AI] 공격 정보 불완전");
                return false;
            }
            
            // BattleActionContext 생성 - Attack은 Skill로 변환
            var actionContext = new BattleActionContext
            {
                battleActionType = ActionType.Skill,  // BattleAction의 ActionType.Skill
                actor = _owner,
                battleField = _stageField,
                skillModel = set.SkillToUse,
                TargetCell = set.TargetCell.Value,
                targets = new List<CharBase> { set.TargetChar }
            };
            
            // SkillBattleAction 실행
            var skillAction = new SkillBattleAction(actionContext);
            var result = await skillAction.ExecuteAction(CancellationToken.None);
            
            if (result.ActionSuccess)
            {
                _currentTurn.TryUseMajorAction();
                Debug.Log($"[AI] 공격 성공: {set.SkillToUse.SkillName} → {set.TargetChar.name}");
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// 푸시 실행
        /// </summary>
        private async UniTask<bool> ExecutePush(AIActionSet set)
        {
            if (!_currentTurn.CanPerformAction(TurnActionState.ActionCategory.MajorAction))
            {
                Debug.LogWarning("[AI] 주요 행동 사용 불가");
                return false;
            }
            
            if (!set.TargetCell.HasValue)
            {
                Debug.LogWarning("[AI] 푸시 대상 없음");
                return false;
            }
            
            // BattleActionContext 생성
            var actionContext = new BattleActionContext
            {
                battleActionType = ActionType.Push,  // BattleAction의 ActionType.Push
                actor = _owner,
                battleField = _stageField,
                TargetCell = set.TargetCell.Value
            };
            
            // PushBattleAction 실행
            var pushAction = new PushBattleAction(actionContext);
            var result = await pushAction.ExecuteAction(CancellationToken.None);
            
            if (result.ActionSuccess)
            {
                _currentTurn.TryUseMajorAction();
                Debug.Log($"[AI] 푸시 성공: {set.TargetCell.Value}");
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// 점프 실행
        /// </summary>
        private async UniTask<bool> ExecuteJump(AIActionSet set)
        {
            if (!_currentTurn.CanPerformAction(TurnActionState.ActionCategory.MajorAction))
            {
                Debug.LogWarning("[AI] 주요 행동 사용 불가");
                return false;
            }
            
            if (!set.TargetCell.HasValue)
            {
                Debug.LogWarning("[AI] 점프 대상 없음");
                return false;
            }
            
            // BattleActionContext 생성
            var actionContext = new BattleActionContext
            {
                battleActionType = ActionType.Jump,  // BattleAction의 ActionType.Jump
                actor = _owner,
                battleField = _stageField,
                TargetCell = set.TargetCell.Value
            };
            
            // JumpBattleAction 실행
            var jumpAction = new JumpBattleAction(actionContext);
            var result = await jumpAction.ExecuteAction(CancellationToken.None);
            
            if (result.ActionSuccess)
            {
                _currentTurn.TryUseMajorAction();
                Debug.Log($"[AI] 점프 성공: {set.TargetCell.Value}");
                return true;
            }
            
            return false;
        }
        
        #endregion
    }
}