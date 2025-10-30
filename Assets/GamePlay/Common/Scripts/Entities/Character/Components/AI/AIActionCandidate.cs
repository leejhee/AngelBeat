using System;
using UnityEngine;

namespace GamePlay.Character.Components
{
    /// <summary>
    /// AI 행동 후보 하나를 나타내는 클래스
    /// PDF 2-3단계: 행동 후보 + 우선도 계산
    /// </summary>
    public class AIActionCandidate
    {
        public enum ActionType
        {
            Attack,     // 공격 (스킬 사용)
            Move,       // 이동 (적에게 접근)
            Defend,     // 방어 (회피/후퇴)
            Buff        // 버프/강화 (미구현)
        }
        
        public ActionType Action { get; private set; }
        public int BasePriority { get; private set; }
        public int Modifier { get; private set; }
        public int TotalPriority => BasePriority + Modifier;
        
        public AIActionCandidate(ActionType action, int basePriority, int modifier = 0)
        {
            Action = action;
            BasePriority = basePriority;
            Modifier = modifier;
        }
        
        public override string ToString()
        {
            return $"{Action} (Priority: {TotalPriority} = {BasePriority} + {Modifier})";
        }
    }
    
    /// <summary>
    /// AI 행동 후보 생성 및 우선도 계산 팩토리
    /// PDF 기준 우선도 테이블 구현
    /// </summary>
    public static class AIActionCandidateFactory
    {
        private const int ATTACK_BASE = 50;
        private const int MOVE_BASE = 30;
        private const int DEFEND_BASE = 40;
        private const int BUFF_BASE = 35;
        
        private const int ATTACK_MOD = 20;
        private const int MOVE_MOD = 10;
        private const int DEFEND_MOD = 20;
        private const int BUFF_MOD = 15;
        
        /// <summary>
        /// AIContext를 기반으로 모든 행동 후보 생성
        /// </summary>
        public static AIActionCandidate[] GenerateCandidates(AIContext context)
        {
            var candidates = new System.Collections.Generic.List<AIActionCandidate>();
            
            // 1. 공격 (canAttack이면 +20)
            int attackMod = context.CanAttack ? ATTACK_MOD : 0;
            candidates.Add(new AIActionCandidate(
                AIActionCandidate.ActionType.Attack, 
                ATTACK_BASE, 
                attackMod
            ));
            
            // 2. 이동 (!canAttack이면 +10)
            int moveMod = !context.CanAttack ? MOVE_MOD : 0;
            candidates.Add(new AIActionCandidate(
                AIActionCandidate.ActionType.Move, 
                MOVE_BASE, 
                moveMod
            ));
            
            // 3. 방어 (lowHP이면 +20)
            int defendMod = context.LowHP ? DEFEND_MOD : 0;
            candidates.Add(new AIActionCandidate(
                AIActionCandidate.ActionType.Defend, 
                DEFEND_BASE, 
                defendMod
            ));
            
            // 4. 버프 (grouped이면 +15) - 미구현
            int buffMod = context.Grouped ? BUFF_MOD : 0;
            candidates.Add(new AIActionCandidate(
                AIActionCandidate.ActionType.Buff, 
                BUFF_BASE, 
                buffMod
            ));
            
            return candidates.ToArray();
        }
        
        /// <summary>
        /// 후보 중 최고 우선도 행동 선택
        /// </summary>
        public static AIActionCandidate SelectBestAction(AIActionCandidate[] candidates)
        {
            if (candidates == null || candidates.Length == 0)
            {
                Debug.LogWarning("[AI] 행동 후보가 없습니다.");
                return null;
            }
            
            AIActionCandidate best = candidates[0];
            foreach (var candidate in candidates)
            {
                if (candidate.TotalPriority > best.TotalPriority)
                {
                    best = candidate;
                }
            }
            
            Debug.Log($"[AI] 최종 선택: {best}");
            return best;
        }
        
        /// <summary>
        /// 우선도 순으로 정렬된 행동 목록 반환 (실패 시 다음 후보 시도용)
        /// </summary>
        public static AIActionCandidate[] GetSortedCandidates(AIActionCandidate[] candidates)
        {
            var sorted = new AIActionCandidate[candidates.Length];
            Array.Copy(candidates, sorted, candidates.Length);
            Array.Sort(sorted, (a, b) => b.TotalPriority.CompareTo(a.TotalPriority));
            return sorted;
        }
    }
}