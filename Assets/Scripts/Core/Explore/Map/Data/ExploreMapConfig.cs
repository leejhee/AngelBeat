using AngelBeat.Core.Character;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Explore.Map
{
    public class ExploreParameter
    {
        public Party PlayerParty;
        public ExploreMapConfig MapConfig;
    }
    
    /// <summary>
    /// 탐사 맵 config 클래스
    /// </summary>
    [Serializable, CreateAssetMenu(fileName = "ExploreMapConfig", menuName = "ScriptableObject/ExploreMapConfig")]
    public class ExploreMapConfig : ScriptableObject
    {
        [Header("던전 이름")]
        public SystemEnum.eDungeon dungeonName;
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
        public List<SystemEnum.eEvent> eventCandidate;
    }
    
    
    /// <summary>
    /// 탐사 맵 생성기
    /// </summary>
    public static class ExploreMapGenerator
    {
        
    }
    
}