using Core.Scripts.Foundation.Define;
using Core.Scripts.Foundation.Singleton;
using GamePlay.Common.Scripts.Entities.Character;
using UnityEngine;

namespace GamePlay.Features.Battle.Scripts
{
    public class BattlePayload : SingletonObject<BattlePayload>{
        public Party PlayerParty { get; private set; }
        public SystemEnum.Dungeon DungeonName { get; private set; }
        public string StageName { get; private set; }
        public SystemEnum.eScene ReturningScene { get; private set; }
        private BattlePayload() { }

        public void SetBattleData(Party party, SystemEnum.Dungeon dungeon, string stageName = null,
            SystemEnum.eScene returningScene = SystemEnum.eScene.ExploreScene)
        {
            PlayerParty = party;
            DungeonName = dungeon;
            StageName = stageName;
            ReturningScene = returningScene;
            Debug.Log($"{party}");
        }

        public void Clear()
        {
            PlayerParty = null;
            DungeonName = default;
        }
    }
}