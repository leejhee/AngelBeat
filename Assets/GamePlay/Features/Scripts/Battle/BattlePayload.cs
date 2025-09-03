using Core.Scripts.Foundation;
using Core.Scripts.Foundation.Define;
using Core.Scripts.Foundation.Singleton;
using GamePlay.Entities.Scripts.Character;
using UnityEngine;

namespace GamePlay.Features.Scripts.Battle
{
    public class BattlePayload : SingletonObject<BattlePayload>{
        public Party PlayerParty { get; private set; }
        public SystemEnum.Dungeon DungeonName { get; private set; }
        public string StageName { get; private set; }

        private BattlePayload() { }
        
        public void SetBattleData(Party party, SystemEnum.Dungeon dungeon, string stageName=null)
        {
            PlayerParty = party;
            DungeonName = dungeon;
            StageName = stageName;
            
            Debug.Log($"{party}");
        }

        public void Clear()
        {
            PlayerParty = null;
            DungeonName = default;
        }
    }
}