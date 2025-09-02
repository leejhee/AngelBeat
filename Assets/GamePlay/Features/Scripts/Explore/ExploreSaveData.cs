using Core.Scripts.Foundation.Define;
using GamePlay.Character;
using GamePlay.Entities.Scripts.Character;
using GamePlay.Explore.Map.Logic;
using System;
using UnityEngine.Serialization;

namespace GamePlay.Explore
{
    [Serializable]
    public class ExploreSaveData
    {
        [FormerlySerializedAs("Dungeon")] public SystemEnum.Dungeon dungeon;
        public Party PlayerParty;
        public ExploreMap MapData;
        
        public ExploreSaveData(
            Party playerParty,
            SystemEnum.Dungeon dungeon
            //TODO : 추후 인벤토리 넣어야 한다. 레벨스케일링 데이터도!
        )
        {
            this.dungeon = dungeon;
            PlayerParty = playerParty;
        }
        
        // TODO : 탐사에서의 저장 포인트를 알아올 것. 호출 타이밍을 정해야 한다.
        public void WriteMap(ExploreMap map)
        {
            MapData = map;
        }
    }
}