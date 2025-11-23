using Cysharp.Threading.Tasks;
using GamePlay.Common.Scripts.Contracts;
using GamePlay.Features.Battle.Scripts.BattleAction;
using GamePlay.Features.Battle.Scripts.BattleMap;
using GamePlay.Features.Battle.Scripts.BattleTurn;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace GamePlay.Features.Battle.Scripts.Unit.Components.AI
{
    /// <summary>
    /// 적 AI 제어 (ActionSet 기반)
    /// PDF "적 AI 판단 로직 (Simple Ver.)" 완전 구현
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
        /// PDF 전체 플로우 실행
        /// </summary>
        /// 
        public async UniTask ExecuteTurn(Turn turn)
        {
            _currentTurn = turn;
            await UniTask.Delay(1000); // 연출용 딜레이
            
            #region Grid Initialization
            _stageField = BattleController.Instance.StageField;
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
            
            // 1단계: 상황 분석
            _context = new AIContext(_owner, _grid);
            _context.AnalyzeSituation();
            Debug.Log(_context.GetSummary());
            
            // 2단계: 모든 ActionSet 생성
            _setGenerator = new AIActionSetGenerator(_context);
            List<AIActionSet> allSets = _setGenerator.GenerateAllActionSets();
            
            // 3단계: 각 세트에 재이동 설정
            foreach (var set in allSets)
            {
                _setGenerator.CheckAfterMoveForSet(set);
            }
            
            // 4단계: 유효성 필터링
            List<AIActionSet> validSets = _setGenerator.FilterInvalidSets(allSets);
            Debug.Log($"[AI] 유효한 세트: {validSets.Count}/{allSets.Count}");
            
            if (validSets.Count == 0)
            {
                Debug.LogWarning($"[AI] {_owner.name} 실행 가능한 행동 없음, 턴 종료");
                await UniTask.Delay(500);
                return;
            }
            
            // 5단계: 가중치 계산
            foreach (var set in validSets)
            {
                _setGenerator.CalculateWeight(set);
            }
            
            // 6단계: 상위 세트 선택
            List<AIActionSet> topSets = _setGenerator.SelectTopSets(validSets, 3);
            
            // 7단계: 실행
            bool actionSuccess = false;
            foreach (var set in topSets)
            {
                Debug.Log($"[AI] 시도: {set}");
                actionSuccess = await TryExecuteActionSet(set);
                
                if (actionSuccess)
                {
                    Debug.Log($"[AI] ✓ 성공: {set}");
                    break;
                }
                
                Debug.Log("[AI] ✗ 실패, 다음 후보 시도");
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
        /// PDF 7단계: ActionSet 실행 시도
        /// </summary>
        private async UniTask<bool> TryExecuteActionSet(AIActionSet set)
        {
            try
            {
                // 0. 방향 전환 (타겟이 있는 경우)
                if (set.TargetChar && set.TargetCell.HasValue)
                {
                    AdjustDirection(set.TargetCell.Value);
                }
                
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
        
        #region 행동 실행 메서드들
        
        /// <summary>
        /// 타겟 방향으로 캐릭터 방향 조정
        /// </summary>
        private void AdjustDirection(Vector2Int targetCell)
        {
            Vector2Int currentCell = _grid.WorldToCell(_owner.CharTransform.position);
            
            bool shouldFaceRight = targetCell.x > currentCell.x;
            
            if (_owner.LastDirectionRight != shouldFaceRight)
            {
                _owner.LastDirectionRight = shouldFaceRight;
                // TODO: 실제 스프라이트 플립 적용 필요
                Debug.Log($"[AI] 방향 전환: {(shouldFaceRight ? "→" : "←")}");
            }
        }
        
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
            
            // BattleActionContext 생성
            var actionContext = new BattleActionContext
            {
                battleActionType = ActionType.Move,
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
                Debug.Log($"[AI] ✓ 이동 성공: {currentPos} → {target}");
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// 공격 실행
        /// </summary>
        private async UniTask<bool> ExecuteAttack(AIActionSet set)
        {
            if (!_currentTurn.CanPerformAction(TurnActionState.ActionCategory.SkillAction))
            {
                Debug.LogWarning("[AI] 주요 행동 사용 불가");
                return false;
            }
            
            if (set.SkillToUse == null || !set.TargetChar|| !set.TargetCell.HasValue)
            {
                Debug.LogWarning("[AI] 공격 정보 불완전");
                return false;
            }
            
            // BattleActionContext 생성
            var actionContext = new BattleActionContext
            {
                battleActionType = ActionType.Skill,
                actor = _owner,
                battleField = _stageField,
                skillModel = set.SkillToUse,
                TargetCell = set.TargetCell.Value,
                targets = new List<IDamageable> { set.TargetChar }
            };
            
            // SkillBattleAction 실행
            var skillAction = new SkillBattleAction(actionContext);
            var result = await skillAction.ExecuteAction(CancellationToken.None);
            
            if (result.ActionSuccess)
            {
                _currentTurn.TryUseSkill();
                Debug.Log($"[AI] ✓ 공격 성공: {set.SkillToUse.SkillName} → {set.TargetChar.name}");
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// 푸시 실행
        /// </summary>
        private async UniTask<bool> ExecutePush(AIActionSet set)
        {
            if (!_currentTurn.CanPerformAction(TurnActionState.ActionCategory.SkillAction))
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
                battleActionType = ActionType.Push,
                actor = _owner,
                battleField = _stageField,
                TargetCell = set.TargetCell.Value
            };
            
            // PushBattleAction 실행
            var pushAction = new PushBattleAction(actionContext);
            var result = await pushAction.ExecuteAction(CancellationToken.None);
            
            if (result.ActionSuccess)
            {
                _currentTurn.TryUseSkill();
                Debug.Log($"[AI] ✓ 푸시 성공: {set.TargetCell.Value}");
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// 점프 실행
        /// </summary>
        private async UniTask<bool> ExecuteJump(AIActionSet set)
        {
            if (!_currentTurn.CanPerformAction(TurnActionState.ActionCategory.SkillAction))
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
                battleActionType = ActionType.Jump,
                actor = _owner,
                battleField = _stageField,
                TargetCell = set.TargetCell.Value
            };
            
            // JumpBattleAction 실행
            var jumpAction = new JumpBattleAction(actionContext);
            var result = await jumpAction.ExecuteAction(CancellationToken.None);
            
            if (result.ActionSuccess)
            {
                _currentTurn.TryUseSkill();
                Debug.Log($"[AI] ✓ 점프 성공: {set.TargetCell.Value}");
                return true;
            }
            
            return false;
        }
        
        #endregion
    }
}