using AngelBeat.Core.Character;
using UnityEditor.SceneManagement;

namespace AngelBeat.Core.SingletonObjects
{
    public class BattlePayload : SingletonObject<BattlePayload>{
        public Party PlayerParty { get; private set; }
        public SystemEnum.eDungeon DungeonName { get; private set; }
        public string StageName { get; private set; }

        private BattlePayload() { }
        
        public void SetBattleData(Party party, SystemEnum.eDungeon dungeon, string stageName=null)
        {
            PlayerParty = party;
            DungeonName = dungeon;
            StageName = stageName;
        }

        public void Clear()
        {
            PlayerParty = null;
            DungeonName = default;
        }
    }
}