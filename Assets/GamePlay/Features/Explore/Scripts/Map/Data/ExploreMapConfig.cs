using Core.Scripts.Foundation.Define;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay.Features.Explore.Scripts.Map.Data
{
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

        [Header("지나갈 수 있는 땅")] 
        public GameObject floorPrefab;
        [Header("경계 역할의 땅")] 
        public GameObject wallPrefab;
        
        [Header("던전 내 심볼 수 테이블")]
        public List<SymbolConfigEntry> symbolConfig;
        [Header("심볼 내 아이템 후보 ID 테이블")]
        public List<ItemConfigEntry> itemCandidate;
        [Header("심볼 내 이벤트 후보 ENUM 리스트")]
        public List<EventConfigEntry> eventCandidate;
    }
}