using Core.Foundation.Define;
using GamePlay.Character;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay.Explore.Map.Data
{
    /// <summary>
    /// 탐사를 시작시키기 위한 파라미터
    /// </summary>
    public class ExploreParameter
    {
        public Party PlayerParty;
        public SystemEnum.Dungeon DungeonName;
        public int FloorNum;

        public ExploreParameter(Party playerParty, SystemEnum.Dungeon dungeonName, int FloorNum)
        {
            PlayerParty = playerParty;
            this.DungeonName = dungeonName;
            this.FloorNum = FloorNum;
        }
    }


    /// <summary>
    /// 탐사 맵 config 클래스
    /// </summary>
    [Serializable, CreateAssetMenu(fileName = "ExploreMapConfig", menuName = "ScriptableObject/ExploreMapConfig")]
    public class ExploreMapConfig : ScriptableObject
    {
        [Header("던전 이름")]
        public SystemEnum.Dungeon dungeonName;
        [Header("던전 층")]
        public int floor;
        
        [Header("던전 X방향 타일")]
        public int xCapacity;
        [Header("던전 Y방향 타일")]
        public int yCapacity;
        
        [Header("던전 내 이벤트 심볼 개수")]
        public int eventSymbolCount;
        [Header("던전 내 아이템 심볼 개수")]
        public int itemSymbolCount;
        [Header("던전 내 전투 심볼 개수")]
        public int battleSymbolCount;
        
        [Header("심볼 내 아이템 후보 ID 리스트")]
        public List<int> itemCandidate;
        [Header("심볼 내 이벤트 후보 ENUM 리스트")]
        public List<SystemEnum.CellEventType> eventCandidate;
    }
}