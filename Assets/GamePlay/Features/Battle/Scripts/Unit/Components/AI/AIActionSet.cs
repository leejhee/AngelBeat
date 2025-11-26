using GamePlay.Common.Scripts.Entities.Skills;
using UnityEngine;

namespace GamePlay.Features.Battle.Scripts.Unit.Components.AI
{
    public enum AIActionType
    {
        Attack,     // 스킬 공격
        Push,       // 밀치기
        Jump,       // 점프
        Move,       // 단순 이동만 하는 경우
        Wait        // 이동도 안하고 대기만 하는 경우(moveto가 null이어야만 함)
    }
    
    /// <summary>
    /// 이동 - 행동 - 재이동
    /// </summary>
    public class AIActionSet
    {
        // 1차 이동 정보
        public Vector2Int? MoveTo { get; set; }           // "어디로 이동할 건지". null이면 현재 위치에서 실행
        
        // 행동 정보
        public AIActionType AIActionType { get; set; }
        public SkillModel SkillToUse { get; set; }        // ActionType.Attack일 때 사용
        public Vector2Int? TargetCell { get; set; }       // 행동 타겟 위치
        public CharBase TargetChar { get; set; }          // 타겟 캐릭터
        
        // 2차 이동 정보
        public Vector2Int? AfterMove { get; set; }        // null이면 재이동 없음
        
        // 가중치
        public float Weight { get; set; }

        

        public override string ToString()
        {
            string moveStr = MoveTo.HasValue ? $"Move({MoveTo.Value})" : "Stay";
            string actionStr = AIActionType.ToString();
            if (AIActionType == AIActionType.Attack && SkillToUse != null)
            {
                actionStr = $"Attack[{SkillToUse.SkillName}]→{(TargetChar ? TargetChar.name : "?")}";
            }
            
            else if (AIActionType == AIActionType.Push && TargetChar)
            {
                actionStr = $"Push[{TargetChar.name}]";
            }
            else if (AIActionType == AIActionType.Jump && TargetCell.HasValue)
            {
                actionStr = $"Jump→{TargetCell.Value}";
            }
            
            string afterStr = AfterMove.HasValue ? $"→AfterMove({AfterMove.Value})" : "";
            
            return $"[W:{Weight:F1}] {moveStr} → {actionStr} {afterStr}";
        }
    }
    
    /// <summary>
    /// 가중치 계산 관련 상수
    /// </summary>
    public static class AIWeightConstants
    {
        // Base 가중치
        public const float ATTACK_BASE = 80f;
        public const float PUSH_BASE = 45f;
        public const float JUMP_BASE = 40f;
        public const float MOVE_BASE = 30f;
        public const float WAIT_BASE = 10f;
        
        // Situation 보정치
        public const float LOW_HP_BONUS = 20f;          // HP 30% 이하
        public const float GROUPED_BONUS = 15f;         // 주변 아군 2명 이상
        public const float CAN_ATTACK_BONUS = 20f;      // 공격 가능 상태
        
        // Risk 보정치
        public const float SAFE_POSITION_BONUS = 10f;   // 적 사거리 밖
        public const float DANGER_POSITION_PENALTY = -20f; // 적 사거리 안
        
        // Position 보정치
        public const float NEAR_CENTER_BONUS = 5f;      // 중앙에 가까움
        public const float NEAR_FALL_PENALTY = -15f;    // 낙사 인접
        public const float TARGET_KILLABLE_BONUS = 30f; // 타겟을 처치 가능
        public const float TARGET_LOW_HP_BONUS = 15f;   // 타겟 HP 낮음 (30% 이하)
    }
}