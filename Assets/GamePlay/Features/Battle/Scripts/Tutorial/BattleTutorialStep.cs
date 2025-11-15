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
        
        [Header("실행할 연출")]
        public string novelScriptId;   
    }
}