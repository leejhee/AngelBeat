using Core.Scripts.Foundation.Define;
using Core.Scripts.Foundation.Singleton;
using GamePlay.Common.Scripts.Entities.Character;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay.Features.Battle.Scripts
{
    public class BattlePayload : SingletonObject<BattlePayload>{
        public Party PlayerParty { get; private set; }
        public SystemEnum.Dungeon DungeonName { get; private set; }
        public List<string> StageNames { get; private set; }
        public SystemEnum.eScene ReturningScene { get; private set; }
        
        public int CurrentStageIndex { get; private set; }
        public string CurrentStageName =>
            (StageNames != null &&
             CurrentStageIndex >= 0 &&
             CurrentStageIndex < StageNames.Count)
                ? StageNames[CurrentStageIndex]
                : null;
        
        public int StageCount => StageNames?.Count ?? 0;
        public bool HasAnyStage => StageCount > 0;
        public bool HasNextStage => HasAnyStage && CurrentStageIndex + 1 < StageCount;
        
        #region Tutorial Section

        public const int TutorialMaxCount = 3;
        public int CurrentTutorialIndex { get; private set; }
        public bool NeedTutorial => CurrentTutorialIndex < TutorialMaxCount;
        #endregion
        
        private BattlePayload() { }

        public void SetBattleData(
            Party party, 
            SystemEnum.Dungeon dungeon, 
            List<string> stageNames = null,
            SystemEnum.eScene returningScene = SystemEnum.eScene.ExploreScene)
        {
            PlayerParty = party;
            DungeonName = dungeon;
            
            StageNames ??= new List<string>();
            StageNames.Clear();
            if (stageNames != null)
                StageNames.AddRange(stageNames);
            
            ReturningScene = returningScene;
            CurrentStageIndex = 0;
            CurrentTutorialIndex = 0;
            
            Debug.Log($"{party}");
        }
        
        public bool MoveToNextStage()
        {
            if (!HasNextStage)
                return false;

            CurrentStageIndex++;
            CurrentTutorialIndex++;
            return true;
        }
        
        public void RestartCurrentStage()
        {
            // 인덱스를 바꾸지 않음
        }
        
        
        public void Clear()
        {
            PlayerParty = null;
            DungeonName = default;

            StageNames?.Clear();
            CurrentStageIndex = 0;
            CurrentTutorialIndex = 0;
            ReturningScene = SystemEnum.eScene.ExploreScene;
        }
    }
}