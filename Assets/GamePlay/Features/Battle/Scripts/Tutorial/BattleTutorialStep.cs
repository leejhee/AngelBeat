using GamePlay.Features.Battle.Scripts.BattleAction;
using UnityEngine;

namespace GamePlay.Features.Battle.Scripts.Tutorial
{
    [CreateAssetMenu(
        menuName = "GamePlay/Battle/Tutorial Step",
        fileName = "BattleTutorialStep")]
    public class BattleTutorialStep : ScriptableObject
    {
        [Header("ID 및 중복 실행 옵션")] 
        public string stepID;
        public bool triggerOnce = true;
        
        [Header("트리거 타입")]
        public TutorialTriggerEventType triggerType;
        
        [Header("연출 타입")]
        public BattleTutorialViewType viewType = BattleTutorialViewType.Novel;
        
        [Header("라운드 조건 - 0이면 무시")]
        public int requiredRound = 0;
        
        [Header("캐릭터 조건")]
        public bool onlyPlayer = false;        // 플레이어만
        public bool onlyEnemy = false;         // 적만
        public int requiredCharId = 0;         
        public int requiredActorTurnCount = 0;
        
        [Header("행동 조건 - ActionCompleted에서만 사용")]
        public bool filterActionType = false;
        public ActionType requiredActionType;
        
        public bool filterSkillId = false;
        public int requiredSkillId;
        
        [Header("Novel 연출이라면 Script의 ID를 작성할 것")]
        public string novelScriptId;   
        
        [Header("Guide 연출이라면 Page의 배열을 작성할 것")]
        public BattleTutorialGuidePage[] guidePages;
        
        [Header("Guide 연출이라면 입력 유도 / 강제 옵션")]
        public bool lockInputDuringStep = false;  // 이 스텝이 활성화된 동안 입력을 제한할지
        // 무엇을 클릭하게 할지
        public TutorialGuideTarget requiredClickTarget = TutorialGuideTarget.None;
        // 액터 기준 오프셋
        public Vector2Int requiredCellOffset;

    }
}