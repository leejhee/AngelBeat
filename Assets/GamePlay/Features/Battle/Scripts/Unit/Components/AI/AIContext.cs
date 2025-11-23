using Core.Scripts.Foundation.Define;
using GamePlay.Features.Battle.Scripts.BattleMap;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay.Features.Battle.Scripts.Unit.Components.AI
{
    /// <summary>
    /// AI가 한 턴 동안 상황을 판단한 결과를 저장하는 컨텍스트
    /// PDF 1단계: 상황 분석
    /// </summary>
    public class AIContext
    {
        // AI 주체
        public CharBase Self { get; private set; }
        
        // 그리드 시스템 (필수)
        public BattleStageGrid Grid { get; private set; }
        
        // 상황 판단 플래그들 (PDF 1단계)
        public bool CanAttack { get; private set; }      // 사거리 내 적 존재 (더 이상 현재 위치만 체크하지 않음)
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
        /// PDF 1단계: 전장 상황을 분석하여 플래그 설정
        /// </summary>
        public void AnalyzeSituation()
        {
            // 현재 위치
            CurrentCell = Grid.WorldToCell(Self.CharTransform.position);
            
            // 이동력 계산
            AvailableMoveRange = (int)Self.RuntimeStat.GetStat(SystemEnum.eStats.NACTION_POINT);
            
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
            
            // 1. canAttack: 이제 Generator에서 직접 판단하므로 간단히 거리 기반으로만
            //    실제로는 이동 후 각 위치에서 공격 가능 여부를 Generator가 체크
            CanAttack = CheckIfInAttackRange();
            
            // 2. lowHP: 체력이 30% 이하인지
            LowHP = Self.CurrentHP <= Self.MaxHP * 0.3f;
            
            // 3. grouped: 주변 2칸 내 아군이 2명 이상인지
            Grouped = CheckIfGrouped();
            
            Debug.Log($"[AIContext] ====== 상황 분석 ======");
            Debug.Log($"  위치: {CurrentCell}, 이동력: {AvailableMoveRange}");
            Debug.Log($"  가장 가까운 적: {(NearestEnemy ? NearestEnemy.name : "없음")} (거리: {GridDistanceToNearestEnemy})");
            Debug.Log($"  HP: {Self.CurrentHP:F0}/{Self.MaxHP:F0} (LowHP: {LowHP})");
            Debug.Log($"  주변 아군: {NearbyAllies.Count}명 (Grouped: {Grouped})");
            Debug.Log($"  공격 가능 범위 내: {CanAttack}");
        }
        
        /// <summary>
        /// 실제 걸어서 이동 가능한 칸 계산 (MoveBattleAction 로직 기반)
        /// 🔧 수정: 장애물/유닛이 있으면 즉시 탐색 중단
        /// </summary>
        private void CalculateWalkableCells()
        {
            WalkableCells.Clear();
            
            // 오른쪽 탐색
            for (int offset = 1; offset <= AvailableMoveRange; offset++)
            {
                Vector2Int candidate = new Vector2Int(CurrentCell.x + offset, CurrentCell.y);
                
                // 맵 밖이면 중단
                if (Grid.IsMaskable(candidate)) break;
                
                // 갈 수 있으면 추가
                if (Grid.IsWalkable(candidate))
                {
                    WalkableCells.Add(candidate);
                }
                else
                {
                    // 갈 수 없는 칸(장애물/유닛)이면 탐색 중단
                    break;
                }
            }
            
            // 왼쪽 탐색
            for (int offset = 1; offset <= AvailableMoveRange; offset++)
            {
                Vector2Int candidate = new Vector2Int(CurrentCell.x - offset, CurrentCell.y);
                
                // 맵 밖이면 중단
                if (Grid.IsMaskable(candidate)) break;
                
                // 갈 수 있으면 추가
                if (Grid.IsWalkable(candidate))
                {
                    WalkableCells.Add(candidate);
                }
                else
                {
                    // 갈 수 없는 칸(장애물/유닛)이면 탐색 중단
                    break;
                }
            }
            
            Debug.Log($"[AIContext] 이동 가능 칸: {string.Join(", ", WalkableCells)}");
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
        /// 간단한 공격 가능 거리 체크 (대략적 판단용)
        /// 실제 공격 가능 여부는 Generator에서 각 위치마다 정확히 계산
        /// </summary>
        private bool CheckIfInAttackRange()
        {
            if (!NearestEnemy) return false;
            
            // 이동력 + 평균 스킬 사거리(3칸 가정)를 고려한 대략적 판단
            int estimatedMaxRange = AvailableMoveRange + 3;
            
            bool inRange = GridDistanceToNearestEnemy <= estimatedMaxRange;
            
            return inRange;
        }
        
        /// <summary>
        /// 주변 2칸 내 아군이 2명 이상인지 확인
        /// </summary>
        private bool CheckIfGrouped()
        {
            NearbyAllies.Clear();
            
            // 같은 편 캐릭터 목록 가져오기
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