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
    /// AI ActionSet 생성 및 필터링, 가중치 계산
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
        
        /// <summary>
        /// 모든 ActionSet 생성
        /// </summary>
        public List<AIActionSet> GenerateAllActionSets()
        {
            List<AIActionSet> allSets = new List<AIActionSet>();
            
            // 현재 위치에서 가능한 행동
            allSets.AddRange(GenerateSetsAtPosition(_context.CurrentCell));
            
            // 각 이동 가능 위치에서 가능한 행동
            foreach (var movePos in _context.WalkableCells)
            {
                allSets.AddRange(GenerateSetsAtPosition(movePos));
            }
            
            Debug.Log($"[AISetGen] 총 {allSets.Count}개 ActionSet 생성됨");
            return allSets;
        }
        
        /// <summary>
        /// 특정 위치에서 가능한 모든 행동 세트 생성
        /// </summary>
        private List<AIActionSet> GenerateSetsAtPosition(Vector2Int position)
        {
            List<AIActionSet> sets = new List<AIActionSet>();
            
            // 1. 공격 세트들
            sets.AddRange(GenerateAttackSetsAt(position));
            
            // 2. 푸시 세트들
            sets.AddRange(GeneratePushSetsAt(position));
            
            // 3. 점프 세트들 (현재 위치에서만)
            if (position == _context.CurrentCell)
            {
                sets.AddRange(GenerateJumpSets());
            }
            
            // 4. 단순 이동 세트 (목표가 이동 위치인 경우)
            if (position != _context.CurrentCell)
            {
                sets.Add(new AIActionSet
                {
                    MoveTo = position,
                    AIActionType = AIActionType.Move,
                    TargetCell = position
                });
            }
            
            // 5. 대기 세트 (현재 위치)
            if (position == _context.CurrentCell)
            {
                sets.Add(new AIActionSet
                {
                    MoveTo = null,
                    AIActionType = AIActionType.Wait
                });
            }
            
            return sets;
        }
        
        /// <summary>
        /// 특정 위치에서 공격 가능한 모든 세트 생성
        /// </summary>
        private List<AIActionSet> GenerateAttackSetsAt(Vector2Int position)
        {
            List<AIActionSet> sets = new();
            
            // 모든 스킬에 대해
            foreach (var skill in _self.SkillInfo.SkillSlots)
            {
                if (skill == null) continue;
                if (skill.skillType != SystemEnum.eSkillType.PhysicalAttack &&
                    skill.skillType != SystemEnum.eSkillType.MagicAttack) continue;
                
                // 해당 위치에서 이 스킬로 공격 가능한 적들 찾기
                var targetsInRange = FindTargetsInRangeFrom(position, skill);
                
                foreach (var target in targetsInRange)
                {
                    Vector2Int targetCell = _grid.WorldToCell(target.CharTransform.position);
                    
                    var set = new AIActionSet
                    {
                        MoveTo = (position != _context.CurrentCell) ? position : (Vector2Int?)null,
                        AIActionType = AIActionType.Attack,
                        SkillToUse = skill,
                        TargetCell = targetCell,
                        TargetChar = target
                    };
                    
                    sets.Add(set);
                }
            }
            
            return sets;
        }
        
        /// <summary>
        /// 특정 위치에서 스킬 사거리 내의 적 목록 반환
        /// </summary>
        private List<CharBase> FindTargetsInRangeFrom(Vector2Int position, SkillModel skill)
        {
            List<CharBase> targets = new List<CharBase>();
            
            try
            {
                // 임시로 캐릭터 위치를 변경해서 범위 계산
                Vector3 originalPos = _self.CharTransform.position;
                _self.CharTransform.position = _grid.CellToWorldCenter(position);
                
                // SkillRangeHelper로 범위 계산
                BattleActionPreviewData rangeData = SkillRangeHelper.ComputeSkillRange(
                    _grid,
                    skill.skillRange,
                    _self
                );
                
                // 원래 위치로 복구
                _self.CharTransform.position = originalPos;
                
                // 범위 내의 적 찾기 (HP로 생존 여부 확인)
                foreach (var enemy in _context.AllEnemies)
                {
                    if (!enemy || enemy.CurrentHP <= 0) continue;
                    
                    Vector2Int enemyCell = _grid.WorldToCell(enemy.CharTransform.position);
                    if (rangeData.PossibleCells.Contains(enemyCell))
                    {
                        targets.Add(enemy);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[AISetGen] 타겟 탐색 오류: {e.Message}");
            }
            
            return targets;
        }
        
        /// <summary>
        /// 특정 위치에서 푸시 가능한 세트 생성
        /// </summary>
        private List<AIActionSet> GeneratePushSetsAt(Vector2Int position)
        {
            List<AIActionSet> sets = new List<AIActionSet>();
            
            // 좌우 인접한 적 확인
            Vector2Int[] pushDirections = { Vector2Int.left, Vector2Int.right };
            
            foreach (var dir in pushDirections)
            {
                Vector2Int pushTarget = position + dir;
                
                if (!_grid.IsOccupied(pushTarget)) continue;
                
                CharBase victim = _grid.GetUnitAt(pushTarget);
                if (!victim || victim.GetCharType() == _self.GetCharType()) continue;
                if (victim.CurrentHP <= 0) continue; // 죽은 적 제외
                
                var set = new AIActionSet
                {
                    MoveTo = (position != _context.CurrentCell) ? position : (Vector2Int?)null,
                    AIActionType = AIActionType.Push,
                    TargetCell = pushTarget,
                    TargetChar = victim
                };
                
                sets.Add(set);
            }
            
            return sets;
        }
        
        /// <summary>
        /// 점프 세트 생성
        /// </summary>
        private List<AIActionSet> GenerateJumpSets()
        {
            List<AIActionSet> sets = new List<AIActionSet>();
            
            foreach (var jumpCell in _context.JumpableCells)
            {
                var set = new AIActionSet
                {
                    MoveTo = null, // 점프는 현재 위치에서 바로
                    AIActionType = AIActionType.Jump,
                    TargetCell = jumpCell
                };
                
                sets.Add(set);
            }
            
            return sets;
        }
        
        /// <summary>
        /// PDF 3단계: 행동 후 재이동 판단
        /// </summary>
        public void CheckAfterMoveForSet(AIActionSet set)
        {
            // 행동 후 위치 계산
            Vector2Int positionAfterAction = set.MoveTo ?? _context.CurrentCell;
            
            if (set.AIActionType == AIActionType.Jump && set.TargetCell.HasValue)
            {
                positionAfterAction = set.TargetCell.Value;
            }
            
            // 행동 후 남은 이동력 계산 (간단히 1칸 이동 가능한지만 체크)
            // TODO: 실제 AP 소비량 계산 필요
            
            // 안전한 재배치 위치 찾기
            Vector2Int? safeRetreat = FindSafeRetreatFrom(positionAfterAction);
            
            if (safeRetreat.HasValue)
            {
                set.AfterMove = safeRetreat.Value;
            }
        }
        
        /// <summary>
        /// 특정 위치에서 안전한 후퇴 위치 찾기
        /// </summary>
        private Vector2Int? FindSafeRetreatFrom(Vector2Int from)
        {
            // 좌우 1칸만 체크
            Vector2Int[] retreatCandidates = { from + Vector2Int.left, from + Vector2Int.right };
            
            Vector2Int? best = null;
            float maxSafety = -1f;
            
            foreach (var candidate in retreatCandidates)
            {
                if (!_grid.IsWalkable(candidate)) continue;
                
                // 안전도 평가 (적과의 거리)
                float safety = EvaluatePositionSafety(candidate);
                
                if (safety > maxSafety)
                {
                    maxSafety = safety;
                    best = candidate;
                }
            }
            
            return best;
        }
        
        /// <summary>
        /// PDF 4단계: 불필요한 세트 필터링
        /// </summary>
        public List<AIActionSet> FilterInvalidSets(List<AIActionSet> sets)
        {
            return sets.Where(s => IsValidSet(s)).ToList();
        }
        
        /// <summary>
        /// 세트 유효성 검증
        /// </summary>
        private bool IsValidSet(AIActionSet set)
        {
            // 1. 타겟이 필요한데 없는 경우
            if ((set.AIActionType == AIActionType.Attack || 
                 set.AIActionType == AIActionType.Push) &&
                !set.TargetChar)
            {
                return false;
            }
            
            // 2. 타겟이 이미 죽어있는 경우
            if (set.TargetChar && set.TargetChar.CurrentHP <= 0)
            {
                return false;
            }
            
            // 3. 낙사 위치
            if (set.MoveTo.HasValue && !_grid.IsPlatform(set.MoveTo.Value))
            {
                return false;
            }
            
            if (set.AfterMove.HasValue && !_grid.IsPlatform(set.AfterMove.Value))
            {
                return false;
            }
            
            // 4. 자신을 타겟으로 하는 경우
            if (set.TargetChar == _self)
            {
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// PDF 5단계: 가중치 계산
        /// </summary>
        public void CalculateWeight(AIActionSet set)
        {
            float weight = 0f;
            
            // Base 가중치
            weight += GetBaseWeight(set.AIActionType);
            
            // Situation 보정
            weight += GetSituationBonus();
            
            // Risk 보정
            weight += GetRiskModifier(set);
            
            // Position 보정
            weight += GetPositionBonus(set);
            
            set.Weight = weight;
        }
        
        private float GetBaseWeight(AIActionType actionType)
        {
            switch (actionType)
            {
                case AIActionType.Attack:
                    return AIWeightConstants.ATTACK_BASE;
                case AIActionType.Push:
                    return AIWeightConstants.PUSH_BASE;
                case AIActionType.Jump:
                    return AIWeightConstants.JUMP_BASE;
                case AIActionType.Move:
                    return AIWeightConstants.MOVE_BASE;
                case AIActionType.Wait:
                    return AIWeightConstants.WAIT_BASE;
                default:
                    return 0f;
            }
        }
        
        private float GetSituationBonus()
        {
            float bonus = 0f;
            
            if (_context.LowHP)
            {
                bonus += AIWeightConstants.LOW_HP_BONUS;
            }
            
            if (_context.Grouped)
            {
                bonus += AIWeightConstants.GROUPED_BONUS;
            }
            
            if (_context.CanAttack)
            {
                bonus += AIWeightConstants.CAN_ATTACK_BONUS;
            }
            
            return bonus;
        }
        
        private float GetRiskModifier(AIActionSet set)
        {
            // 행동 후 최종 위치
            Vector2Int finalPos = set.AfterMove ?? set.MoveTo ?? _context.CurrentCell;
            
            float safety = EvaluatePositionSafety(finalPos);
            
            if (safety > 3f) // 적과 거리 3칸 이상
            {
                return AIWeightConstants.SAFE_POSITION_BONUS;
            }
            else if (safety < 2f) // 적과 거리 2칸 이하
            {
                return AIWeightConstants.DANGER_POSITION_PENALTY;
            }
            
            return 0f;
        }
        
        private float GetPositionBonus(AIActionSet set)
        {
            float bonus = 0f;
            
            // 타겟 관련 보너스
            if (set.TargetChar && set.TargetChar.CurrentHP > 0)
            {
                float targetHPPercent = set.TargetChar.CurrentHP / set.TargetChar.MaxHP;
                
                if (targetHPPercent <= 0.3f)
                {
                    bonus += AIWeightConstants.TARGET_LOW_HP_BONUS;
                    
                    // 처치 가능한지 예측 (간단히 스킬 데미지로 판단)
                    if (set.AIActionType == AIActionType.Attack && set.SkillToUse != null)
                    {
                        // TODO: 실제 데미지 계산 필요
                        // 임시로 저체력이면 처치 가능하다고 가정
                        if (targetHPPercent <= 0.2f)
                        {
                            bonus += AIWeightConstants.TARGET_KILLABLE_BONUS;
                        }
                    }
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
        
        /// <summary>
        /// 위치의 안전도 평가 (가장 가까운 적과의 거리)
        /// </summary>
        private float EvaluatePositionSafety(Vector2Int position)
        {
            if (_context.AllEnemies.Count == 0)
                return 10f; // 매우 안전
            
            float minDistance = float.MaxValue;
            
            foreach (var enemy in _context.AllEnemies)
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
        
        /// <summary>
        /// 낙사 인접 여부 체크
        /// </summary>
        private bool IsNearFall(Vector2Int position)
        {
            Vector2Int[] adjacent = { position + Vector2Int.left, position + Vector2Int.right };
            
            foreach (var adj in adjacent)
            {
                if (!_grid.IsInBounds(adj) || !_grid.IsPlatform(adj))
                {
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// PDF 6단계: 정렬 및 상위 추출
        /// </summary>
        public List<AIActionSet> SelectTopSets(List<AIActionSet> sets, int count = 3)
        {
            // 가중치 내림차순 정렬
            var sorted = sets.OrderByDescending(s => s.Weight).ToList();
            
            // 상위 N개 추출
            int takeCount = Mathf.Min(count, sorted.Count);
            var topSets = sorted.Take(takeCount).ToList();
            
            Debug.Log($"[AISetGen] Top {takeCount} Sets:");
            foreach (var set in topSets)
            {
                Debug.Log($"  {set}");
            }
            
            return topSets;
        }
    }
}