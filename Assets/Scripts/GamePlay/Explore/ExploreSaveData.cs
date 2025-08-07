using GamePlay.Character;
using GamePlay.Explore.Map.Logic;
using System;

namespace GamePlay.Explore
{
    [Serializable]
    public class ExploreSaveData
    {
        public SystemEnum.eDungeon Dungeon;
        public Party PlayerParty;
        public ExploreMap MapData;
        
        public ExploreSaveData(
            Party playerParty,
            SystemEnum.eDungeon dungeon
            //TODO : 추후 인벤토리 넣어야 한다. 레벨스케일링 데이터도!
        )
        {
            Dungeon = dungeon;
            PlayerParty = playerParty;
        }
        
        // TODO : 탐사에서의 저장 포인트를 알아올 것. 호출 타이밍을 정해야 한다.
        public void WriteMap(ExploreMap map)
        {
            MapData = map;
        }
    }
}