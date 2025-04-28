using AngelBeat.Core.Character.Party;

namespace AngleBeat.Core.SingletonObjects
{
    public class BattlePayload : SingletonObject<BattlePayload>{
        public Party PlayerParty { get; private set; }
        public string MapName { get; private set; }

        private BattlePayload() { }

        public void SetBattleData(Party party, string mapName)
        {
            PlayerParty = party;
            MapName = mapName;
        }

        public void Clear()
        {
            PlayerParty = null;
            MapName = null;
        }
    }
}