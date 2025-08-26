using AngelBeat.Core.SingletonObjects;
using Core.Foundation.Define;
using GamePlay.Character;

namespace GamePlay.Battle
{
    public interface IBattleStageSource
    {
        public SystemEnum.Dungeon Dungeon { get; }
        public Party PlayerParty { get; }
        public string StageName { get; }
    }

    public class BattlePayloadSource : IBattleStageSource
    {
        public SystemEnum.Dungeon Dungeon => BattlePayload.Instance.DungeonName;
        public Party PlayerParty => BattlePayload.Instance.PlayerParty;
        public string StageName => BattlePayload.Instance.StageName;
        
        public void ClearPayload() => BattlePayload.Instance.Clear();
    }

    public class DebugMockSource : IBattleStageSource
    {
        private readonly SystemEnum.Dungeon _dungeon;
        private readonly Party _playerParty;
        private readonly string _stageName;
        
        public SystemEnum.Dungeon Dungeon => _dungeon;
        public Party PlayerParty => _playerParty;
        public string StageName => _stageName;
        
        public DebugMockSource(SystemEnum.Dungeon dungeon, Party playerParty,  string stageName)
        {
            _dungeon = dungeon;
            _playerParty = playerParty;
            _stageName = stageName;
        }
    }
}