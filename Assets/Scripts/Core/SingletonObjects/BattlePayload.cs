using AngelBeat.Core.Character.Party;

namespace AngleBeat.Core.SingletonObjects
{
    public class BattlePayload : SingletonObject<BattlePayload>{
        public Party PlayerParty { get; private set; }
        public SystemEnum.eDungeon DungeonName { get; private set; }

        private BattlePayload() { }
        
        public void SetBattleData(Party party, SystemEnum.eDungeon dungeon)
        {
            PlayerParty = party;
            DungeonName = dungeon;
        }

        public void Clear()
        {
            PlayerParty = null;
            DungeonName = default;
        }
    }
}