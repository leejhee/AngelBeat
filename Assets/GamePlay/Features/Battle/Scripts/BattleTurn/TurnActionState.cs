using UnityEngine;

namespace GamePlay.Features.Battle.Scripts.BattleTurn
{
    public class TurnActionState
    {
        // 이동력 관리
        private float _maxMovePoint;
        private float _remainingMovePoint;
        
        // 주요 행동 사용 여부
        private bool _majorActionUsed;
        
        // 행동 타입 정의
        public enum ActionCategory
        {
            Move,           // 이동 
            SkillAction,    // 스킬 
            ExtraAction     // 밀기 / 점프
        }
        
        public float MaxMovePoint => _maxMovePoint;
        public float RemainingMovePoint => _remainingMovePoint;
        public bool MajorActionUsed => _majorActionUsed;
        
        /// <summary>
        /// 턴 시작 시 행동 상태 초기화
        /// </summary>
        public void Initialize(float movePoint)
        {
            _maxMovePoint = movePoint;
            _remainingMovePoint = movePoint;
            _majorActionUsed = false;
        }
        
        /// <summary>
        /// 행동 수행 가능 여부 검증
        /// </summary>
        public bool CanPerformAction(ActionCategory category, int moveDistance)
        {
            switch (category)
            {
                case ActionCategory.Move: //남아는 있어야 하고, moveDistance 이상으로 남아야 하니까.
                    return _remainingMovePoint > 0 && _remainingMovePoint >= moveDistance;
                    
                case ActionCategory.SkillAction:
                    return !_majorActionUsed;
                    
                default:
                    return false;
            }
        }
        
        /// <summary>
        /// 이동력 소모 (이동 시 호출)
        /// </summary>
        public bool ConsumeMovePoint(float amount)
        {
            if (_remainingMovePoint < amount)
            {
                Debug.LogWarning($"이동력 부족: 필요 {amount}, 남은 {_remainingMovePoint}");
                return false;
            }
            
            _remainingMovePoint -= amount;
            Debug.Log($"이동력 소모: {amount} (남은 이동력: {_remainingMovePoint}/{_maxMovePoint})");
            return true;
        }
        
        /// <summary>
        /// 주요 행동 사용 (밀기/점프/스킬 실행 시 호출)
        /// </summary>
        public bool UseMajorAction()
        {
            if (_majorActionUsed)
            {
                Debug.LogWarning("이미 주요 행동을 사용했습니다 (밀기/점프/스킬은 턴당 1회)");
                return false;
            }
            
            _majorActionUsed = true;
            Debug.Log("주요 행동 사용됨 (이번 턴에 추가 밀기/점프/스킬 불가)");
            return true;
        }
        
        /// <summary>
        /// 현재 행동 가능 상태 요약 (디버깅/UI용)
        /// </summary>
        public string GetStatusSummary()
        {
            return $"이동력: {_remainingMovePoint:F1}/{_maxMovePoint:F1} | " +
                   $"주요행동: {(_majorActionUsed ? "사용완료" : "사용가능")}";
        }
    }
}