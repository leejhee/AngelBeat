using Core.Scripts.Foundation.Define;
using Core.Scripts.Foundation.Singleton;
using GamePlay.Common.Scripts.Entities.Character;

namespace GamePlay.Features.Explore.Scripts
{
    /// <summary>
    /// 씬 전환 시 탐사 초기화에 필요한 정보를 전달하는 싱글턴 페이로드
    /// </summary>
    public class ExplorePayload : SingletonObject<ExplorePayload>
    {
        private ExplorePayload() { }
        
        public SystemEnum.Dungeon TargetDungeon { get; private set; }
        public int TargetFloor { get; private set; }
        public Party PlayerParty { get; private set; }
        public bool IsNewExplore { get; private set; }
        
        /// <summary>
        /// 새 탐사 시작 
        /// </summary>
        public void SetNewExplore(SystemEnum.Dungeon dungeon, int floor, Party party)
        {
            TargetDungeon = dungeon;
            TargetFloor = floor;
            PlayerParty = party;
            IsNewExplore = true;
        }
        
        /// <summary>
        /// 탐사 이어하기
        /// </summary>
        public void SetContinueExplore(SystemEnum.Dungeon dungeon, int floor, Party party)
        {
            TargetDungeon = dungeon;
            TargetFloor = floor;
            PlayerParty = party;
            IsNewExplore = false;
        }
        
        /// <summary>
        /// 탐사 종료 및 필요 없을 때 날리는 용도
        /// </summary>
        public void Clear()
        {
            TargetDungeon = SystemEnum.Dungeon.None;
            TargetFloor = 0;
            PlayerParty = null;
            IsNewExplore = false;
        }
    }
}