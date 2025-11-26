using Core.Scripts.Foundation.Define;
using GamePlay.Features.Battle.Scripts.BattleAction;
using GamePlay.Features.Battle.Scripts.BattleMap;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay.Features.Battle.Scripts.Unit.Components.AI
{
    /// <summary>
    /// AI가 한 턴 동안 상황을 판단한 결과를 저장하는 컨텍스트
    /// </summary>
    public class AIContext
    {
        // AI 주체
        public CharBase Actor { get; private set; }
        
        // 그리드 시스템 (필수)
        public BattleStageGrid Grid { get; private set; }
        
        public bool LowHP { get; private set; }
        public bool Grouped { get; private set; }        
        //
        //// 분석된 전장 정보
        public CharBase PrimaryTarget { get; private set; }
        public Vector2Int PrimaryTargetCell { get; private set; }
        public bool HasPrimaryTarget => PrimaryTarget != null;
        public List<CharBase> NearbyAllies { get; private set; }
        public List<CharBase> AllEnemies { get; private set; }
        //
        //// 이동 정보
        public Vector2Int CurrentCell { get; private set; }
        public int AvailableMoveRange { get; private set; }  // 현재 행동력
        //public Dictionary<Vector2Int, MoveState> ReachableStates { get; private set; }
        public List<Vector2Int> MovableCells { get; private set; }  // 실제 이동 가능한 칸
        //public Dictionary<MoveKey, MoveParent?> MoveParents { get; private set; }
        
        public AIContext(CharBase actor, BattleStageGrid grid)
        {
            Actor = actor;
            Grid = grid;
            MovableCells = new List<Vector2Int>();
            
            NearbyAllies = new List<CharBase>();
            AllEnemies = new List<CharBase>();
        }

        public void AnalyzeSituation()
        {
            // 현재 셀 초기화
            CurrentCell = Grid.WorldToCell(Actor.CharTransform.position);
            
            // 점프를 포함, 행동 이전에 이동 가능한 모든 경우 계산 및 저장
            CalculateMovableCells();
            
            // 전체 적 목록 가져오기
            AllEnemies = BattleCharManager.Instance.GetEnemies(
                BattleCharManager.GetEnemyType(Actor.GetCharType())
            );
            
            // lowHP: 체력이 30% 이하인지
            LowHP = Actor.CurrentHP <= Actor.MaxHP * 0.3f;
            
            SelectPrimaryTarget();
            /*
           Debug.Log($"[AIContext] ====== 상황 분석 ======");
           Debug.Log($"  위치: {CurrentCell}, 이동력: {AvailableMoveRange}");
           Debug.Log($"  가장 가까운 적: {(NearestEnemy ? NearestEnemy.name : "없음")} (거리: {GridDistanceToNearestEnemy})");
           Debug.Log($"  HP: {Self.CurrentHP:F0}/{Self.MaxHP:F0} (LowHP: {LowHP})");
           Debug.Log($"  주변 아군: {NearbyAllies.Count}명 (Grouped: {Grouped})");
           Debug.Log($"  공격 가능 범위 내: {CanAttack}");*/
        }
        
        /// <summary>
        /// 이동 가능한 전체 범위 세팅
        /// </summary>
        private void CalculateMovableCells()
        {
            BattleActionPreviewData moveData = BattleRangeHelper.ComputeMoveRangeFromClient(Grid, Actor);
            MovableCells = moveData.PossibleCells;
            MovableCells.Add(CurrentCell); // 현재 좌표를 포함해야한다.
            Debug.Log($"[AIContext] 이동 가능 칸: {string.Join(", ", MovableCells)}");
            
        }
        
        private void SelectPrimaryTarget()
        {
            float bestDist = float.MaxValue;
            CharBase best = null;
            Vector2Int bestCell = default;

            foreach (var enemy in AllEnemies)
            {
                if (!enemy || enemy.CurrentHP <= 0) continue;

                var cell = Grid.WorldToCell(enemy.CharTransform.position);
                float dist = Mathf.Abs(cell.x - CurrentCell.x)
                             + Mathf.Abs(cell.y - CurrentCell.y);

                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = enemy;
                    bestCell = cell;
                }
            }

            PrimaryTarget = best;
            PrimaryTargetCell = bestCell;
        }
        
        /// </summary>
        //private bool CheckIfInAttackRange()
        //{
        //    //if (!NearestEnemy) return false;
        //    //
        //    //// 이동력 + 평균 스킬 사거리(3칸 가정)를 고려한 대략적 판단
        //    //int estimatedMaxRange = AvailableMoveRange + 3;
        //    //
        //    //bool inRange = GridDistanceToNearestEnemy <= estimatedMaxRange;
        //    //
        //    //return inRange;
        //}
        

        //private bool CheckIfGrouped()
        //{
            //NearbyAllies.Clear();
            //
            //// 같은 편 캐릭터 목록 가져오기
            //var allCharacters = BattleCharManager.Instance.GetBattleMembers();
            //
            //// 거리 2칸 이내의 아군 카운트 (그리드 맨해튼 거리)
            //const int groupRange = 2;
            //
            //foreach (var character in allCharacters)
            //{
            //    if (character == Self) continue; // 자기 자신 제외
            //    if (character.GetCharType() != Self.GetCharType()) continue; // 같은 편만
            //    
            //    Vector2Int allyCell = Grid.WorldToCell(character.CharTransform.position);
            //    int manhattanDistance = Mathf.Abs(allyCell.x - CurrentCell.x) + 
            //                            Mathf.Abs(allyCell.y - CurrentCell.y);
            //    
            //    if (manhattanDistance <= groupRange)
            //    {
            //        NearbyAllies.Add(character);
            //    }
            //}
            //
            //return NearbyAllies.Count >= 2;
        //}
        
        /// <summary>
        /// 특정 위치로 이동 가능한지 확인
        /// </summary>
        public bool CanWalkTo(Vector2Int target)
        {
            return MovableCells.Contains(target);
        }
        

        //public bool CanJumpTo(Vector2Int target)
        //{
        //    //return JumpableCells.Contains(target);
        //}
        
        /// <summary>
        /// 목표 방향으로 이동할 최적 셀 찾기
        /// </summary>
        public Vector2Int? FindBestMoveToward(Vector2Int target)
        {
            if (MovableCells.Count == 0) return null;
            
            Vector2Int best = MovableCells[0];
            int bestDistance = int.MaxValue;
            
            foreach (var cell in MovableCells)
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
        

        //public string GetSummary()
        //{
        //    //return $"[AI Summary]\n" +
        //    //       $"  Position: {CurrentCell}\n" +
        //    //       $"  HP: {Self.CurrentHP:F0}/{Self.MaxHP:F0} ({Self.CurrentHP/Self.MaxHP*100:F0}%)\n" +
        //    //       $"  Move Range: {AvailableMoveRange} (Walkable: {MovableCells.Count}, Jumpable: {JumpableCells.Count})\n" +
        //    //       $"  Nearest Enemy: {(NearestEnemy ? NearestEnemy.name : "None")} (Grid Distance: {GridDistanceToNearestEnemy})\n" +
        //    //       $"  Nearby Allies: {NearbyAllies.Count}\n" +
        //    //       $"  Flags: canAttack={CanAttack}, lowHP={LowHP}, grouped={Grouped}";
        //}
    }
}