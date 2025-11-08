using Core.Scripts.Foundation.Define;
using Core.Scripts.Foundation.SceneUtil;
using GamePlay.Common.Scripts.Entities.Character;

namespace GamePlay.Features.Battle.Scripts
{
    public interface IBattleSceneSource : ISceneArgs
    {
        public SystemEnum.Dungeon Dungeon { get; }
        public Party PlayerParty { get; }
        public string StageName { get; }
    }

    public class BattlePayloadSource : IBattleSceneSource
    {
        public SystemEnum.Dungeon Dungeon => BattlePayload.Instance.DungeonName;
        public Party PlayerParty => BattlePayload.Instance.PlayerParty;
        public string StageName => BattlePayload.Instance.StageName;
        
        public void ClearPayload() => BattlePayload.Instance.Clear();
    }

    public class DebugMockSource : IBattleSceneSource
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

        public static DebugMockSource Default()
        {
            Party p = new();
            p.InitParty();
            return new DebugMockSource(SystemEnum.Dungeon.MOUNTAIN_BACK, p, "TestMap1");
        }
    }
}