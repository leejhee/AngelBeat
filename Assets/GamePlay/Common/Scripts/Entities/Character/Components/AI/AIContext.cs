using Core.Scripts.Foundation.Define;
using GamePlay.Common.Scripts.Entities.Skills;
using GamePlay.Features.Battle.Scripts;
using GamePlay.Features.Battle.Scripts.BattleAction;
using GamePlay.Features.Battle.Scripts.BattleMap;
using GamePlay.Features.Battle.Scripts.Unit;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay.Common.Scripts.Entities.Character.Components.AI
{
    /// <summary>
    /// AI가 한 턴 동안 상황을 판단한 결과를 저장하는 컨텍스트
    /// </summary>
    public class AIContext
    {
        // AI 주체
        public CharBase Self { get; private set; }
        
        // 그리드 시스템 (필수)
        public BattleStageGrid Grid { get; private set; }
        
        // 상황 판단 플래그들 (PDF 1단계)
        public bool CanAttack { get; private set; }      // 사거리 내 적 존재
        public bool LowHP { get; private set; }          // HP ≤ 30%
        public bool Grouped { get; private set; }        // 주변 2칸 내 아군 ≥ 2
        
        // 분석된 전장 정보
        public CharBase NearestEnemy { get; private set; }
        public float DistanceToNearestEnemy { get; private set; }
        public int GridDistanceToNearestEnemy { get; private set; }
        public List<CharBase> NearbyAllies { get; private set; }
        public List<CharBase> AllEnemies { get; private set; }
        
        // 이동 정보
        public Vector2Int CurrentCell { get; private set; }
        public int AvailableMoveRange { get; private set; }  // 현재 행동력
        public List<Vector2Int> WalkableCells { get; private set; }  // 실제 이동 가능한 칸
        public List<Vector2Int> JumpableCells { get; private set; }  // 점프 가능한 칸
        
        public AIContext(CharBase self, BattleStageGrid grid)
        {
            Self = self;
            Grid = grid;
            NearbyAllies = new List<CharBase>();
            AllEnemies = new List<CharBase>();
            WalkableCells = new List<Vector2Int>();
            JumpableCells = new List<Vector2Int>();
        }
        
        /// <summary>
        /// 전장 상황을 분석하여 플래그 설정
        /// </summary>
        public void AnalyzeSituation()
        {
            // 현재 위치
            CurrentCell = Grid.WorldToCell(Self.CharTransform.position);
            
            // 이동력 계산
            AvailableMoveRange = (int)Self.RuntimeStat.GetStat(SystemEnum.eStats.NMACTION_POINT);
            
            // 이동 가능한 칸 계산
            CalculateWalkableCells();
            CalculateJumpableCells();
            
            // 적 목록 가져오기
            AllEnemies = BattleCharManager.Instance.GetEnemies(
                BattleCharManager.GetEnemyType(Self.GetCharType())
            );
            
            // 가장 가까운 적 찾기
            NearestEnemy = BattleCharManager.Instance.GetNearestEnemy(Self);
            if (NearestEnemy)
            {
                DistanceToNearestEnemy = Vector3.Distance(
                    Self.CharTransform.position,
                    NearestEnemy.CharTransform.position
                );
                
                // 그리드 거리 계산 (맨해튼 거리)
                Vector2Int enemyCell = Grid.WorldToCell(NearestEnemy.CharTransform.position);
                GridDistanceToNearestEnemy = Mathf.Abs(enemyCell.x - CurrentCell.x) + 
                                              Mathf.Abs(enemyCell.y - CurrentCell.y);
            }
            
            // 1. canAttack: 사거리 내 적 존재 여부
            CanAttack = CheckIfCanAttack();
            
            // 2. lowHP: 체력이 30% 이하인지
            LowHP = Self.CurrentHP <= Self.MaxHP * 0.3f;
            
            // 3. grouped: 주변 2칸 내 아군이 2명 이상인지
            Grouped = CheckIfGrouped();
            
            Debug.Log($"[AI Context] {Self.name} - Pos:{CurrentCell}, MoveRange:{AvailableMoveRange}, " +
                      $"canAttack:{CanAttack}, lowHP:{LowHP}, grouped:{Grouped}");
        }
        
        /// <summary>
        /// 실제 걸어서 이동 가능한 칸 계산 (MoveBattleAction 로직 기반)
        /// </summary>
        private void CalculateWalkableCells()
        {
            WalkableCells.Clear();
            
            // 오른쪽 탐색
            bool blockedRight = false;
            for (int offset = 1; offset <= AvailableMoveRange; offset++)
            {
                Vector2Int candidate = new Vector2Int(CurrentCell.x + offset, CurrentCell.y);
                if (Grid.IsMaskable(candidate)) continue;
                
                if (Grid.IsWalkable(candidate))
                {
                    if (!blockedRight)
                        WalkableCells.Add(candidate);
                }
                else
                {
                    blockedRight = true;
                }
            }
            
            // 왼쪽 탐색
            bool blockedLeft = false;
            for (int offset = 1; offset <= AvailableMoveRange; offset++)
            {
                Vector2Int candidate = new Vector2Int(CurrentCell.x - offset, CurrentCell.y);
                if (Grid.IsMaskable(candidate)) continue;
                
                if (Grid.IsWalkable(candidate))
                {
                    if (!blockedLeft)
                        WalkableCells.Add(candidate);
                }
                else
                {
                    blockedLeft = true;
                }
            }
        }
        
        /// <summary>
        /// 점프 가능한 칸 계산 (JumpBattleAction 로직 기반)
        /// </summary>
        private void CalculateJumpableCells()
        {
            JumpableCells.Clear();
            
            // JumpBattleAction의 JumpableRange 참조
            List<Vector2Int> jumpOffsets = new List<Vector2Int>
            {
                new Vector2Int(-1, -1),
                new Vector2Int(0, -1),
                new Vector2Int(1, -1),
                new Vector2Int(2, 0),
                new Vector2Int(-2, 0),
                new Vector2Int(-1, 1),
                new Vector2Int(0, 1),
                new Vector2Int(1, 1)
            };
            
            foreach (Vector2Int offset in jumpOffsets)
            {
                Vector2Int candidate = CurrentCell + offset;
                if (Grid.IsMaskable(candidate)) continue;
                if (Grid.IsWalkable(candidate))
                {
                    JumpableCells.Add(candidate);
                }
            }
        }
        
        /// <summary>
        /// 보유한 스킬 중 하나라도 사거리 내에 적이 있는지 확인
        /// </summary>
        private bool CheckIfCanAttack()
        {
            if (!NearestEnemy) return false;
            
            Vector2Int enemyCell = Grid.WorldToCell(NearestEnemy.CharTransform.position);
            
            foreach (SkillModel skill in Self.SkillInfo.SkillSlots)
            {
                if (skill == null) continue;
                if (skill.skillType != SystemEnum.eSkillType.PhysicalAttack &&
                    skill.skillType != SystemEnum.eSkillType.MagicAttack) continue;
                
                try
                {
                    BattleActionPreviewData rangeData = SkillRangeHelper.ComputeSkillRange(
                        Grid,
                        skill.skillRange,
                        Self
                    );
                    
                    // 적의 위치가 타격 가능 범위(PossibleTile)에 포함되는지 확인
                    if (rangeData.PossibleCells.Contains(enemyCell))
                    {
                        Debug.Log($"[AI CanAttack] ✓ {Self.name}의 스킬 [{skill.SkillName}]로 공격 가능" +
                                  $"\n  - 적: {NearestEnemy.name} at {enemyCell}" +
                                  $"\n  - AI 위치: {CurrentCell}" +
                                  $"\n  - AI 방향: {(Self.LastDirection ? "Right" : "Left")}" +
                                  $"\n  - 타격 가능 셀 수: {rangeData.PossibleCells.Count}");
                        return true;
                    }
                    else
                    {
                        // 디버그: 왜 공격할 수 없는지 상세 정보
                        Debug.Log($"[AI CanAttack] ✗ {Self.name}의 스킬 [{skill.SkillName}]로 공격 불가" +
                                  $"\n  - 적 위치: {enemyCell}" +
                                  $"\n  - AI 위치: {CurrentCell}" +
                                  $"\n  - AI 방향: {(Self.LastDirection ? "Right" : "Left")}" +
                                  $"\n  - 타격 가능 셀 수: {rangeData.PossibleCells.Count}" +
                                  $"\n  - 스킬 범위: Forward={skill.skillRange.Forward}, " +
                                  $"Backward={skill.skillRange.Backward}, " +
                                  $"UpForward={skill.skillRange.UpForward}, " +
                                  $"UpBackward={skill.skillRange.UpBackward}");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[AI CanAttack] 스킬 범위 계산 중 오류: {skill.SkillName}\n{e}");
                }
            }
            
            Debug.Log($"[AI CanAttack] ✗ {Self.name}는 어떤 스킬로도 {NearestEnemy.name}를 공격할 수 없음" +
                      $"\n  - 적 위치: {enemyCell}" +
                      $"\n  - AI 위치: {CurrentCell}" +
                      $"\n  - 그리드 거리: {GridDistanceToNearestEnemy}");
            return false;
        }
        
        /// <summary>
        /// 주변 2칸 내 아군이 2명 이상인지 확인
        /// </summary>
        private bool CheckIfGrouped()
        {
            NearbyAllies.Clear();
            
            // 같은 편 캐릭터 목록 가져오기 (적의 적의 적 = 아군)
            var allCharacters = BattleCharManager.Instance.GetBattleMembers();
            
            // 거리 2칸 이내의 아군 카운트 (그리드 맨해튼 거리)
            const int groupRange = 2;
            
            foreach (var character in allCharacters)
            {
                if (character == Self) continue; // 자기 자신 제외
                if (character.GetCharType() != Self.GetCharType()) continue; // 같은 편만
                
                Vector2Int allyCell = Grid.WorldToCell(character.CharTransform.position);
                int manhattanDistance = Mathf.Abs(allyCell.x - CurrentCell.x) + 
                                        Mathf.Abs(allyCell.y - CurrentCell.y);
                
                if (manhattanDistance <= groupRange)
                {
                    NearbyAllies.Add(character);
                }
            }
            
            return NearbyAllies.Count >= 2;
        }
        
        /// <summary>
        /// 특정 위치로 이동 가능한지 확인
        /// </summary>
        public bool CanWalkTo(Vector2Int target)
        {
            return WalkableCells.Contains(target);
        }
        
        /// <summary>
        /// 특정 위치로 점프 가능한지 확인
        /// </summary>
        public bool CanJumpTo(Vector2Int target)
        {
            return JumpableCells.Contains(target);
        }
        
        /// <summary>
        /// 목표 방향으로 이동할 최적 셀 찾기
        /// </summary>
        public Vector2Int? FindBestMoveToward(Vector2Int target)
        {
            if (WalkableCells.Count == 0) return null;
            
            Vector2Int best = WalkableCells[0];
            int bestDistance = int.MaxValue;
            
            foreach (var cell in WalkableCells)
            {
                int distance = Mathf.Abs(cell.x - target.x) + Mathf.Abs(cell.y - target.y);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = cell;
                }
            }
            
            return best;
        }
        
        /// <summary>
        /// 디버깅용 상황 요약
        /// </summary>
        public string GetSummary()
        {
            return $"[AI Summary]\n" +
                   $"  Position: {CurrentCell}\n" +
                   $"  HP: {Self.CurrentHP:F0}/{Self.MaxHP:F0} ({Self.CurrentHP/Self.MaxHP*100:F0}%)\n" +
                   $"  Move Range: {AvailableMoveRange} (Walkable: {WalkableCells.Count}, Jumpable: {JumpableCells.Count})\n" +
                   $"  Nearest Enemy: {(NearestEnemy ? NearestEnemy.name : "None")} (Grid Distance: {GridDistanceToNearestEnemy})\n" +
                   $"  Nearby Allies: {NearbyAllies.Count}\n" +
                   $"  Flags: canAttack={CanAttack}, lowHP={LowHP}, grouped={Grouped}";
        }
    }
}