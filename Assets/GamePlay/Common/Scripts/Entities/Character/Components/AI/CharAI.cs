using Cysharp.Threading.Tasks;
using GamePlay.Character.Components;
using GamePlay.Features.Battle.Scripts;
using GamePlay.Features.Battle.Scripts.BattleAction;
using GamePlay.Features.Battle.Scripts.BattleMap;
using GamePlay.Features.Battle.Scripts.BattleTurn;
using GamePlay.Features.Battle.Scripts.Unit;
using System;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace GamePlay.Common.Scripts.Entities.Character.Components.AI
{
    /// <summary>
    /// 적 AI 제어
    /// </summary>
    public class CharAI
    {
        private CharBase _owner;
        private AIContext _context;
        private Turn _currentTurn;
        private BattleStageGrid _grid;
        private StageField _stageField;
        
        public CharAI(CharBase owner)
        {
            _owner = owner;
        }
        
        /// <summary>
        /// 턴 시작 시 AI 실행 (PDF 전체 플로우)
        /// </summary>
        public async UniTask ExecuteTurn(Turn turn)
        {
            _currentTurn = turn;
            
            // BattleController에서 그리드 가져오기
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
            
            // Context 초기화
            _context = new AIContext(_owner, _grid);
            
            Debug.Log($"[AI] {_owner.name} 턴 시작");
            
            _context.AnalyzeSituation();
            Debug.Log(_context.GetSummary());
            
            var candidates = AIActionCandidateFactory.GenerateCandidates(_context);
            var sortedCandidates = AIActionCandidateFactory.GetSortedCandidates(candidates);
            
            Debug.Log("[AI] 행동 후보 목록:");
            foreach (var c in sortedCandidates)
            {
                Debug.Log($"  - {c}");
            }
            
            bool actionSuccess = false;
            foreach (var candidate in sortedCandidates)
            {
                Debug.Log($"[AI] 시도: {candidate.Action}");
                actionSuccess = await TryExecuteAction(candidate);
                
                if (actionSuccess)
                {
                    Debug.Log($"[AI] 성공: {candidate.Action}");
                    break;
                }

                Debug.Log($"[AI] 실패: {candidate.Action}, 다음 후보 시도");
            }
            
            if (!actionSuccess)
            {
                Debug.LogWarning($"[AI] {_owner.name} 모든 행동 실패, 턴 종료");
            }
            
            // 턴 종료 대기 (연출용)
            await UniTask.Delay(500);
            
            Debug.Log($"[AI] {_owner.name} 턴 종료");
        }
        
        /// <summary>
        /// 선택된 행동 실행 시도
        /// </summary>
        private async UniTask<bool> TryExecuteAction(AIActionCandidate candidate)
        {
            switch (candidate.Action)
            {
                case AIActionCandidate.ActionType.Attack:
                    return await ExecuteAttack();
                
                case AIActionCandidate.ActionType.Move:
                    return await ExecuteMove();
                
                case AIActionCandidate.ActionType.Defend:
                    return await ExecuteDefend();
                
                case AIActionCandidate.ActionType.Buff:
                    return await ExecuteBuff();
                
                default:
                    Debug.LogWarning($"[AI] 알 수 없는 행동: {candidate.Action}");
                    return false;
            }
        }
        
        #region 행동 실행 메서드들
        
        /// <summary>
        /// 공격 실행: 사용 가능한 스킬 중 하나를 선택하여 가장 가까운 적 공격
        /// </summary>
        private async UniTask<bool> ExecuteAttack()
        {
            // 주요 행동 사용 가능한지 확인
            if (!_currentTurn.CanPerformAction(TurnActionState.ActionCategory.MajorAction))
            {
                Debug.LogWarning("[AI] 이미 주요 행동을 사용했습니다.");
                return false;
            }
            
            // 공격 가능한 적이 있는지 확인
            if (_context.NearestEnemy == null)
            {
                Debug.LogWarning("[AI] 공격할 대상이 없습니다.");
                return false;
            }
            
            // 사용 가능한 스킬 중 첫 번째 선택
            var availableSkills = _owner.SkillInfo.SkillSlots.Where(s => s != null).ToList();
            if (availableSkills.Count == 0)
            {
                Debug.LogWarning("[AI] 사용 가능한 스킬이 없습니다.");
                return false;
            }
            
            var selectedSkill = availableSkills[0];
            Vector2Int targetCell = _grid.WorldToCell(_context.NearestEnemy.CharTransform.position);
            
            // BattleActionContext 생성 (BattleController 패턴 참조)
            var actionContext = new BattleActionContext
            {
                battleActionType = ActionType.Skill,
                actor = _owner,
                battleField = _stageField,
                skillModel = selectedSkill,
                TargetCell = targetCell,
                targets = new System.Collections.Generic.List<CharBase> { _context.NearestEnemy }
            };
            
            // SkillBattleAction 생성 및 실행
            var skillAction = new SkillBattleAction(actionContext);
            
            try
            {
                var result = await skillAction.ExecuteAction(CancellationToken.None);
                
                if (result.ActionSuccess)
                {
                    // 주요 행동 소모
                    _currentTurn.TryUseMajorAction();
                    Debug.Log($"[AI] 스킬 사용 성공: {selectedSkill.SkillName} → {_context.NearestEnemy.name}");
                    return true;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            
            return false;
        }
        
        /// <summary>
        /// 이동 실행: 가장 가까운 적에게 접근 (MoveBattleAction 사용)
        /// </summary>
        private async UniTask<bool> ExecuteMove()
        {
            if (_context.NearestEnemy == null)
            {
                Debug.LogWarning("[AI] 이동할 목표가 없습니다.");
                return false;
            }
            
            // 적 방향으로 최선의 이동 위치 찾기
            Vector2Int enemyCell = _grid.WorldToCell(_context.NearestEnemy.CharTransform.position);
            Vector2Int? targetCell = _context.FindBestMoveToward(enemyCell);
            
            if (targetCell == null)
            {
                Debug.LogWarning("[AI] 이동 가능한 위치가 없습니다.");
                return false;
            }
            
            Vector2Int moveTarget = targetCell.Value;
            
            // 이동 거리 계산
            float moveDistance = Mathf.Abs(moveTarget.x - _context.CurrentCell.x);
            
            // 이동력 검증
            if (!_currentTurn.CanPerformAction(TurnActionState.ActionCategory.Move, moveDistance))
            {
                Debug.LogWarning("[AI] 이동력이 부족합니다.");
                return false;
            }
            
            // BattleActionContext 생성
            var actionContext = new BattleActionContext
            {
                battleActionType = ActionType.Move,
                actor = _owner,
                battleField = _stageField,
                TargetCell = moveTarget
            };
            
            // MoveBattleAction 생성 및 실행
            var moveAction = new MoveBattleAction(actionContext);
            
            try
            {
                var result = await moveAction.ExecuteAction(CancellationToken.None);
                
                if (result.ActionSuccess)
                {
                    // 이동력 소모
                    _currentTurn.TryConsumeMove(moveDistance);
                    Debug.Log($"[AI] 이동 성공: {_context.CurrentCell} → {moveTarget}");
                    return true;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            
            return false;
        }
        
        /// <summary>
        /// 방어 실행: 적에게서 멀어지기 (점프 또는 이동)
        /// </summary>
        private async UniTask<bool> ExecuteDefend()
        {
            if (!_context.NearestEnemy)
            {
                Debug.Log("[AI] 방어: 위협 없음, 대기");
                await UniTask.Delay(500);
                return true;
            }
            
            Vector2Int enemyCell = _grid.WorldToCell(_context.NearestEnemy.CharTransform.position);
            
            // 1. 점프로 후퇴 시도
            if (_currentTurn.CanPerformAction(TurnActionState.ActionCategory.MajorAction) && 
                _context.JumpableCells.Count > 0)
            {
                // 적에게서 가장 먼 점프 위치 찾기
                Vector2Int? bestJump = FindFarthestCell(_context.JumpableCells, enemyCell);
                
                if (bestJump != null && await TryJump(bestJump.Value))
                {
                    Debug.Log($"[AI] 점프로 후퇴 성공: {bestJump.Value}");
                    return true;
                }
            }
            
            // 2. 이동으로 후퇴 시도
            if (_context.WalkableCells.Count > 0)
            {
                // 적에게서 가장 먼 이동 위치 찾기
                Vector2Int? bestMove = FindFarthestCell(_context.WalkableCells, enemyCell);
                
                if (bestMove != null)
                {
                    float moveDistance = Mathf.Abs(bestMove.Value.x - _context.CurrentCell.x);
                    
                    if (_currentTurn.CanPerformAction(TurnActionState.ActionCategory.Move, moveDistance))
                    {
                        var actionContext = new BattleActionContext
                        {
                            battleActionType = ActionType.Move,
                            actor = _owner,
                            battleField = _stageField,
                            TargetCell = bestMove.Value
                        };
                        
                        var moveAction = new MoveBattleAction(actionContext);
                        var result = await moveAction.ExecuteAction(CancellationToken.None);
                        
                        if (result.ActionSuccess)
                        {
                            _currentTurn.TryConsumeMove(moveDistance);
                            Debug.Log($"[AI] 이동으로 후퇴 성공: {bestMove.Value}");
                            return true;
                        }
                    }
                }
            }
            
            // 3. 후퇴 불가 시 제자리 대기
            Debug.Log("[AI] 후퇴 불가, 제자리 방어");
            await UniTask.Delay(500);
            return true;
        }
        
        /// <summary>
        /// 점프 실행 헬퍼
        /// </summary>
        private async UniTask<bool> TryJump(Vector2Int target)
        {
            if (!_currentTurn.CanPerformAction(TurnActionState.ActionCategory.MajorAction))
                return false;
            
            var actionContext = new BattleActionContext
            {
                battleActionType = ActionType.Jump,
                actor = _owner,
                battleField = _stageField,
                TargetCell = target
            };
            
            var jumpAction = new JumpBattleAction(actionContext);
            
            try
            {
                var result = await jumpAction.ExecuteAction(CancellationToken.None);
                
                if (result.ActionSuccess)
                {
                    _currentTurn.TryUseMajorAction();
                    return true;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            
            return false;
        }
        
        /// <summary>
        /// 목표에서 가장 먼 셀 찾기
        /// </summary>
        private Vector2Int? FindFarthestCell(System.Collections.Generic.List<Vector2Int> candidates, Vector2Int from)
        {
            if (candidates.Count == 0) return null;
            
            Vector2Int farthest = candidates[0];
            int maxDistance = 0;
            
            foreach (var cell in candidates)
            {
                int distance = Mathf.Abs(cell.x - from.x) + Mathf.Abs(cell.y - from.y);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    farthest = cell;
                }
            }
            
            return farthest;
        }
        
        /// <summary>
        /// 버프 실행: 아군 강화 (현재는 미구현)
        /// </summary>
        private async UniTask<bool> ExecuteBuff()
        {
            Debug.Log("[AI] 버프 행동 (미구현)");
            
            // TODO: 버프 스킬 시스템 구현 후 연동
            await UniTask.Delay(500);
            return false;
        }
        
        #endregion
    }
}