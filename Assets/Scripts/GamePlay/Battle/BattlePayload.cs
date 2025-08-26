using Core.Foundation;
using Core.Foundation.Define;
using GamePlay.Character;
using UnityEngine;

namespace GamePlay.Battle
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