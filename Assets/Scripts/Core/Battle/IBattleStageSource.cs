using AngelBeat.Core.Character;
using AngelBeat.Core.SingletonObjects;

namespace AngelBeat.Core.Battle
{
    public interface IBattleStageSource
    {
        public SystemEnum.eDungeon Dungeon { get; }
        public Party PlayerParty { get; }
        public string StageName { get; }
    }

    public class BattlePayloadSource : IBattleStageSource
    {
        public SystemEnum.eDungeon Dungeon => BattlePayload.Instance.DungeonName;
        public Party PlayerParty => BattlePayload.Instance.PlayerParty;
        public string StageName => BattlePayload.Instance.StageName;
        
        public void ClearPayload() => BattlePayload.Instance.Clear();
    }

    public class DebugMockSource : IBattleStageSource
    {
        private readonly SystemEnum.eDungeon _dungeon;
        private readonly Party _playerParty;
        private readonly string _stageName;
        
        public SystemEnum.eDungeon Dungeon => _dungeon;
        public Party PlayerParty => _playerParty;
        public string StageName => _stageName;
        
        public DebugMockSource(SystemEnum.eDungeon dungeon, Party playerParty,  string stageName)
        {
            _dungeon = dungeon;
            _playerParty = playerParty;
            _stageName = stageName;
        }
    }
}