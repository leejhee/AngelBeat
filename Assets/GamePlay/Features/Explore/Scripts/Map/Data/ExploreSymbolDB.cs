using Core.Scripts.Foundation.Define;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay.Features.Explore.Scripts.Map.Data
{
    [CreateAssetMenu(fileName="ExploreSymbolDB", menuName="ScriptableObject/ExploreSymbolDB")]
    public class ExploreSymbolDB : ScriptableObject
    {
        [Serializable] 
        public struct Entry {
            public SystemEnum.MapCellType cellType; // Start, Boss, Battle, Item, Shop, Event, …
            public GameObject prefab;               // 기본 프리팹
        }
        public List<Entry> entries = new();
        private Dictionary<SystemEnum.MapCellType, GameObject> _dict;

        void OnEnable(){ _dict = new(); foreach (var e in entries) _dict[e.cellType] = e.prefab; }
        public bool TryGet(SystemEnum.MapCellType t, out GameObject pf) => _dict.TryGetValue(t, out pf);
    }
}