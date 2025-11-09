using Core.Scripts.Foundation.Define;
using GamePlay.Common.Scripts.Entities.Skills;
using GamePlay.Features.Battle.Scripts;
using GamePlay.Features.Battle.Scripts.BattleAction;
using GamePlay.Features.Battle.Scripts.BattleMap;
using GamePlay.Features.Battle.Scripts.Unit;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GamePlay.Common.Scripts.Entities.Character.Components.AI
{
    /// <summary>
    /// PDF "적 AI 판단 로직 (Simple Ver.)" 완전 구현
    /// 모든 이동 가능 위치에서 모든 행동을 시뮬레이션
    /// </summary>
    public class AIActionSetGenerator
    {
        private AIContext _context;
        private BattleStageGrid _grid;
        private CharBase _self;
        
        public AIActionSetGenerator(AIContext context)
        {
            _context = context;
            _grid = context.Grid;
            _self = context.Self;
        }
        
        #region PDF 2단계: 각 위치에서 가능한 행동 탐색 (GenerateActionSets)
        
        /// <summary>
        /// PDF 2단계: 모든 이동 가능 위치 × 모든 행동 조합 생성
        /// </summary>
        public List<AIActionSet> GenerateAllActionSets()
        {
            List<AIActionSet> allSets = new();
            
            Debug.Log($"[AISetGen] ====== ActionSet 생성 시작 ======");
            Debug.Log($"[AISetGen] 현재 위치: {_context.CurrentCell}");
            Debug.Log($"[AISetGen] 이동 가능: {_context.WalkableCells.Count}칸, 점프 가능: {_context.JumpableCells.Count}칸");
            Debug.Log($"[AISetGen] 적 수: {_context.AllEnemies.Count}명");
            
            // 🔍 스킬 정보 출력
            Debug.Log($"[AISetGen] 보유 스킬: {_self.SkillInfo.SkillSlots.Count}개");
            foreach (var skill in _self.SkillInfo.SkillSlots)
            {
                if (skill != null)
                {
                    Debug.Log($"  - {skill.SkillName} (타입: {skill.skillType}, 사거리: F{skill.skillRange.Forward}/B{skill.skillRange.Backward})");
                }
            }
            
            // 1. 현재 위치에서 가능한 행동들
            var currentPosSets = GenerateActionsAtPosition(_context.CurrentCell, moveFrom: null);
            allSets.AddRange(currentPosSets);
            Debug.Log($"[AISetGen] 현재 위치 행동: {currentPosSets.Count}개");
            
            // 2. 각 이동 가능 위치에서 가능한 행동들 (이동 → 행동)
            foreach (Vector2Int movePos in _context.WalkableCells)
            {
                var moveSets = GenerateActionsAtPosition(movePos, moveFrom: _context.CurrentCell);
                allSets.AddRange(moveSets);
            }
            Debug.Log($"[AISetGen] 이동 후 행동: {allSets.Count - currentPosSets.Count}개");
            
            // 3. 점프 행동들
            foreach (Vector2Int jumpPos in _context.JumpableCells)
            {
                // 점프는 이동력을 소모하지 않으므로 점프 자체만
                allSets.Add(new AIActionSet
                {
                    MoveTo = null,
                    AIActionType = AIActionType.Jump,
                    TargetCell = jumpPos
                });
                
                // 점프 후 해당 위치에서 가능한 행동들도 추가
                var jumpAfterSets = GenerateActionsAtPosition(jumpPos, moveFrom: null, isAfterJump: true);
                allSets.AddRange(jumpAfterSets);
            }
            Debug.Log($"[AISetGen] 점프 포함: {allSets.Count}개");
            
            // 4. 대기 행동 (현재 위치)
            allSets.Add(new AIActionSet
            {
                MoveTo = null,
                AIActionType = AIActionType.Wait
            });
            
            Debug.Log($"[AISetGen] ====== 총 {allSets.Count}개 생성 완료 ======");
            return allSets;
        }
        
        /// <summary>
        /// 특정 위치에서 수행 가능한 모든 행동 세트 생성
        /// PDF의 핵심: 이동 후 각 위치에서 공격/푸시 가능 여부 체크
        /// </summary>
        private List<AIActionSet> GenerateActionsAtPosition(
            Vector2Int position, 
            Vector2Int? moveFrom,
            bool isAfterJump = false)
        {
            List<AIActionSet> sets = new();
            
            // 1. 공격 행동들 (모든 스킬 × 양방향)
            sets.AddRange(GenerateAttackActions(position, moveFrom, isAfterJump));
            
            // 2. 푸시 행동들
            sets.AddRange(GeneratePushActions(position, moveFrom, isAfterJump));
            
            // 3. 단순 이동만 (행동 없이 이동으로 끝)
            if (moveFrom.HasValue && !isAfterJump)
            {
                sets.Add(new AIActionSet
                {
                    MoveTo = position,
                    AIActionType = AIActionType.Move,
                    TargetCell = position
                });
            }
            
            return sets;
        }
        
        /// <summary>
        /// 특정 위치에서 모든 스킬 × 모든 방향 × 모든 타겟 공격 세트 생성
        /// 핵심: 양방향 모두 시뮬레이션
        /// </summary>
        private List<AIActionSet> GenerateAttackActions(
            Vector2Int position, 
            Vector2Int? moveFrom,
            bool isAfterJump)
        {
            List<AIActionSet> sets = new();
            
            foreach (SkillModel skill in _self.SkillInfo.SkillSlots)
            {
                if (skill == null) continue;
                
                // 공격 스킬만
                if (skill.skillType != SystemEnum.eSkillType.PhysicalAttack &&
                    skill.skillType != SystemEnum.eSkillType.MagicAttack)
                    continue;
                
                // 🔍 각 스킬 처리 로그
                Debug.Log($"[AISetGen] 위치 {position}에서 스킬 [{skill.SkillName}] 체크 중...");
                
                // 좌우 양방향 모두 시도
                foreach (bool faceRight in new[] { true, false })
                {
                    var targets = FindTargetsFromPosition(position, skill, faceRight);
                    
                    Debug.Log($"[AISetGen]   방향 {(faceRight ? "→" : "←")}: 타겟 {targets.Count}명");
                    
                    foreach (CharBase target in targets)
                    {
                        Vector2Int targetCell = _grid.WorldToCell(target.CharTransform.position);
                        
                        var set = new AIActionSet
                        {
                            MoveTo = moveFrom.HasValue ? position : null,
                            AIActionType = isAfterJump ? AIActionType.Jump : AIActionType.Attack,
                            SkillToUse = skill,
                            TargetCell = targetCell,
                            TargetChar = target
                        };
                        
                        sets.Add(set);
                        
                        Debug.Log($"[AISetGen] ✓ 공격 세트 생성: {(moveFrom.HasValue ? $"{moveFrom}→" : "")}{position} " +
                                  $"스킬[{skill.SkillName}] 방향[{(faceRight ? "→" : "←")}] 타겟[{target.name}@{targetCell}]");
                    }
                }
            }
            
            if (sets.Count == 0)
            {
                Debug.LogWarning($"[AISetGen] ✗ 위치 {position}에서 공격 불가");
            }
            
            return sets;
        }
        
        /// <summary>
        /// 특정 위치에서 특정 방향으로 특정 스킬 사용 시 타격 가능한 적 목록
        /// 핵심: Transform 위치와 방향을 임시로 변경하여 SkillRangeHelper 호출
        /// </summary>
        private List<CharBase> FindTargetsFromPosition(
            Vector2Int position, 
            SkillModel skill, 
            bool faceRight)
        {
            List<CharBase> targets = new();
            
            // 원본 저장
            Vector3 originalPos = _self.CharTransform.position;
            bool originalDir = _self.LastDirectionRight;
            
            try
            {
                // 임시 변경
                _self.CharTransform.position = _grid.CellToWorldCenter(position);
                _self.LastDirectionRight = faceRight;
                
                // 🔍 상세 로그
                Debug.Log($"[AISetGen]     임시 위치 설정: {position} (월드: {_self.CharTransform.position})");
                Debug.Log($"[AISetGen]     임시 방향 설정: {(faceRight ? "→" : "←")}");
                
                // 스킬 범위 계산
                BattleActionPreviewData rangeData = SkillRangeHelper.ComputeSkillRange(
                    _grid,
                    skill.skillRange,
                    _self
                );
                
                Debug.Log($"[AISetGen]     범위 계산 완료: 가능 {rangeData.PossibleCells.Count}칸, 불가 {rangeData.BlockedCells.Count}칸");
                Debug.Log($"[AISetGen]     가능 칸: {string.Join(", ", rangeData.PossibleCells)}");
                
                // 범위 내 적 확인
                foreach (CharBase enemy in _context.AllEnemies)
                {
                    if (!enemy || enemy.CurrentHP <= 0) continue;
                    
                    Vector2Int enemyCell = _grid.WorldToCell(enemy.CharTransform.position);
                    
                    Debug.Log($"[AISetGen]     적 {enemy.name} 위치: {enemyCell}");
                    
                    if (rangeData.PossibleCells.Contains(enemyCell))
                    {
                        targets.Add(enemy);
                        Debug.Log($"[AISetGen]     ✓ 타겟 추가: {enemy.name}");
                    }
                    else
                    {
                        Debug.Log($"[AISetGen]     ✗ 범위 밖: {enemy.name}");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[AISetGen] FindTargets 오류: {e.Message}\n{e.StackTrace}");
            }
            finally
            {
                // 복원
                _self.CharTransform.position = originalPos;
                _self.LastDirectionRight = originalDir;
            }
            
            return targets;
        }
        
        /// <summary>
        /// 특정 위치에서 푸시 가능한 행동들
        /// </summary>
        private List<AIActionSet> GeneratePushActions(
            Vector2Int position, 
            Vector2Int? moveFrom,
            bool isAfterJump)
        {
            List<AIActionSet> sets = new();
            
            // 좌우 인접 셀만 푸시 가능
            foreach (Vector2Int dir in new[] { Vector2Int.left, Vector2Int.right })
            {
                Vector2Int targetCell = position + dir;
                
                if (!_grid.IsOccupied(targetCell)) continue;
                
                CharBase victim = _grid.GetUnitAt(targetCell);
                if (!victim || victim.CurrentHP <= 0) continue;
                if (victim.GetCharType() == _self.GetCharType()) continue; // 아군 제외
                
                var set = new AIActionSet
                {
                    MoveTo = moveFrom.HasValue ? position : null,
                    AIActionType = AIActionType.Push,
                    TargetCell = targetCell,
                    TargetChar = victim
                };
                
                sets.Add(set);
                
                Debug.Log($"[AISetGen] 푸시 세트: {(moveFrom.HasValue ? $"{moveFrom}→" : "")}{position} " +
                          $"타겟[{victim.name}@{targetCell}]");
            }
            
            return sets;
        }
        
        #endregion
        
        #region PDF 3단계: 행동 후 재이동 판단 (CheckAfterMove)
        
        /// <summary>
        /// PDF 3단계: 행동 후 남은 이동력으로 재배치 가능 여부 확인
        /// </summary>
        public void CheckAfterMoveForSet(AIActionSet set)
        {
            // 행동 후 위치
            Vector2Int posAfterAction = set.MoveTo ?? _context.CurrentCell;
            
            if (set.AIActionType == AIActionType.Jump && set.TargetCell.HasValue)
            {
                posAfterAction = set.TargetCell.Value;
            }
            
            // TODO: 실제 AP 계산 (현재는 간단히 1칸 재이동 가능한지만 체크)
            // 현재는 이동 → 행동 순서이므로 행동 후 재이동은 제한적
            
            // 재이동 가능한 인접 칸 중 안전한 곳 선택
            Vector2Int? safeRetreat = FindSafeAdjacentCell(posAfterAction);
            
            if (safeRetreat.HasValue)
            {
                set.AfterMove = safeRetreat.Value;
                Debug.Log($"[AISetGen] 재이동 설정: {posAfterAction} → {safeRetreat.Value}");
            }
        }
        
        /// <summary>
        /// 인접한 안전한 셀 찾기
        /// </summary>
        private Vector2Int? FindSafeAdjacentCell(Vector2Int from)
        {
            List<Vector2Int> candidates = new();
            
            foreach (Vector2Int dir in new[] { Vector2Int.left, Vector2Int.right })
            {
                Vector2Int candidate = from + dir;
                
                if (!_grid.IsInBounds(candidate)) continue;
                if (!_grid.IsWalkable(candidate)) continue;
                
                candidates.Add(candidate);
            }
            
            if (candidates.Count == 0) return null;
            
            // 가장 안전한(적과 먼) 곳 선택
            Vector2Int best = candidates[0];
            float maxSafety = EvaluatePositionSafety(best);
            
            foreach (var candidate in candidates)
            {
                float safety = EvaluatePositionSafety(candidate);
                if (safety > maxSafety)
                {
                    maxSafety = safety;
                    best = candidate;
                }
            }
            
            return best;
        }
        
        #endregion
        
        #region PDF 4단계: 불필요한 세트 필터링 (FilterInvalidSets)
        
        /// <summary>
        /// PDF 4단계: 실행 불가능하거나 위험한 세트 제거
        /// </summary>
        public List<AIActionSet> FilterInvalidSets(List<AIActionSet> sets)
        {
            var validSets = sets.Where(s => IsValidSet(s)).ToList();
            
            Debug.Log($"[AISetGen] 필터링: {sets.Count}개 → {validSets.Count}개 (제거: {sets.Count - validSets.Count})");
            
            return validSets;
        }
        
        private bool IsValidSet(AIActionSet set)
        {
            // 1. target이 필요한데 없음
            if ((set.AIActionType == AIActionType.Attack || set.AIActionType == AIActionType.Push) &&
                !set.TargetChar)
            {
                return false;
            }
            
            // 2. 타겟이 죽어있음
            if (set.TargetChar && set.TargetChar.CurrentHP <= 0)
            {
                return false;
            }
            
            // 3. 이동/재이동 위치가 낙사
            if (set.MoveTo.HasValue && !_grid.IsPlatform(set.MoveTo.Value))
            {
                return false;
            }
            
            if (set.AfterMove.HasValue && !_grid.IsPlatform(set.AfterMove.Value))
            {
                return false;
            }
            
            // 4. 자기 자신을 타겟으로
            if (set.TargetChar == _self)
            {
                return false;
            }
            
            // 5. 이동 거리가 이동력 초과 (간단 체크)
            if (set.MoveTo.HasValue)
            {
                int distance = Mathf.Abs(set.MoveTo.Value.x - _context.CurrentCell.x);
                if (distance > _context.AvailableMoveRange)
                {
                    return false;
                }
            }
            
            return true;
        }
        
        #endregion
        
        #region PDF 5단계: 세트 가중치 계산 (CalcWeight)
        
        /// <summary>
        /// PDF 5단계: Weight = Base + Situation + Risk + PositionBonus
        /// </summary>
        public void CalculateWeight(AIActionSet set)
        {
            float weight = 0f;
            
            // Base
            weight += GetBaseWeight(set.AIActionType);
            
            // Situation
            weight += GetSituationBonus();
            
            // Risk
            weight += GetRiskModifier(set);
            
            // Position
            weight += GetPositionBonus(set);
            
            set.Weight = weight;
        }
        
        private float GetBaseWeight(AIActionType actionType)
        {
            return actionType switch
            {
                AIActionType.Attack => AIWeightConstants.ATTACK_BASE,
                AIActionType.Push => AIWeightConstants.PUSH_BASE,
                AIActionType.Jump => AIWeightConstants.JUMP_BASE,
                AIActionType.Move => AIWeightConstants.MOVE_BASE,
                AIActionType.Wait => AIWeightConstants.WAIT_BASE,
                _ => 0f
            };
        }
        
        private float GetSituationBonus()
        {
            float bonus = 0f;
            
            if (_context.LowHP)
                bonus += AIWeightConstants.LOW_HP_BONUS;
            
            if (_context.Grouped)
                bonus += AIWeightConstants.GROUPED_BONUS;
            
            if (_context.CanAttack)
                bonus += AIWeightConstants.CAN_ATTACK_BONUS;
            
            return bonus;
        }
        
        private float GetRiskModifier(AIActionSet set)
        {
            Vector2Int finalPos = set.AfterMove ?? set.MoveTo ?? _context.CurrentCell;
            
            float safety = EvaluatePositionSafety(finalPos);
            
            if (safety >= 3f)
                return AIWeightConstants.SAFE_POSITION_BONUS;
            else if (safety <= 2f)
                return AIWeightConstants.DANGER_POSITION_PENALTY;
            
            return 0f;
        }
        
        private float GetPositionBonus(AIActionSet set)
        {
            float bonus = 0f;
            
            // 타겟 HP 기반 보너스
            if (set.TargetChar && set.TargetChar.CurrentHP > 0)
            {
                float hpPercent = set.TargetChar.CurrentHP / set.TargetChar.MaxHP;
                
                if (hpPercent <= 0.3f)
                {
                    bonus += AIWeightConstants.TARGET_LOW_HP_BONUS;
                }
                
                if (hpPercent <= 0.2f && set.AIActionType == AIActionType.Attack)
                {
                    bonus += AIWeightConstants.TARGET_KILLABLE_BONUS;
                }
            }
            
            // 낙사 인접 페널티
            Vector2Int finalPos = set.AfterMove ?? set.MoveTo ?? _context.CurrentCell;
            if (IsNearFall(finalPos))
            {
                bonus += AIWeightConstants.NEAR_FALL_PENALTY;
            }
            
            return bonus;
        }
        
        private float EvaluatePositionSafety(Vector2Int position)
        {
            if (_context.AllEnemies.Count == 0)
                return 10f;
            
            float minDistance = float.MaxValue;
            
            foreach (CharBase enemy in _context.AllEnemies)
            {
                if (!enemy || enemy.CurrentHP <= 0) continue;
                
                Vector2Int enemyCell = _grid.WorldToCell(enemy.CharTransform.position);
                float distance = Mathf.Abs(enemyCell.x - position.x) + Mathf.Abs(enemyCell.y - position.y);
                
                if (distance < minDistance)
                {
                    minDistance = distance;
                }
            }
            
            return minDistance;
        }
        
        private bool IsNearFall(Vector2Int position)
        {
            foreach (Vector2Int dir in new[] { Vector2Int.left, Vector2Int.right })
            {
                Vector2Int adj = position + dir;
                if (!_grid.IsInBounds(adj) || !_grid.IsPlatform(adj))
                {
                    return true;
                }
            }
            
            return false;
        }
        
        #endregion
        
        #region PDF 6단계: 세트 정렬 및 상위 추출 (SelectTopSets)
        
        /// <summary>
        /// PDF 6단계: 가중치 상위 N개 추출
        /// </summary>
        public List<AIActionSet> SelectTopSets(List<AIActionSet> sets, int count = 3)
        {
            var sorted = sets.OrderByDescending(s => s.Weight)
                             .ThenByDescending(s => GetPriorityOrder(s.AIActionType))
                             .ToList();
            
            int takeCount = Mathf.Min(count, sorted.Count);
            var topSets = sorted.Take(takeCount).ToList();
            
            Debug.Log($"[AISetGen] ====== Top {takeCount} Sets ======");
            for (int i = 0; i < topSets.Count; i++)
            {
                Debug.Log($"  #{i + 1}: {topSets[i]}");
            }
            
            return topSets;
        }
        
        /// <summary>
        /// 가중치 동일 시 우선순위 (공격 > 푸시 > 이동)
        /// </summary>
        private int GetPriorityOrder(AIActionType actionType)
        {
            return actionType switch
            {
                AIActionType.Attack => 3,
                AIActionType.Push => 2,
                AIActionType.Jump => 1,
                AIActionType.Move => 0,
                AIActionType.Wait => -1,
                _ => -2
            };
        }
        
        #endregion
    }
}