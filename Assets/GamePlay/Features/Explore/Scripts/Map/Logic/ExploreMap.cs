using System;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay.Features.Explore.Scripts.Map.Logic
{
    /// <summary>
    /// 맵의 기본 구조를 나타내는 클래스
    /// </summary>
    [Serializable]
    public class ExploreMap
    {
        private readonly Dictionary<Vector2Int, ExploreMapNode> _tileMap = new();
        public IReadOnlyDictionary<Vector2Int, ExploreMapNode> Tiles => _tileMap;

        public List<Vector2Int> FloorCells { get; } = new();
        public List<Vector2Int> BoundCells { get; } = new();

        public struct PlacedSymbol
        {
            public Core.Scripts.Foundation.Define.SystemEnum.CellEventType EventType;
            public Vector2Int Pos;
        }
        public List<PlacedSymbol> Events { get; } = new();
        public List<Vector2Int> Items { get; } = new();
        public List<Vector2Int> Battles { get; } = new();

        public bool TryGetNode(Vector2Int pos, out ExploreMapNode node) => _tileMap.TryGetValue(pos, out node);

        internal void AddNode(ExploreMapNode node, bool isBound)
        {
            _tileMap[node.Pos] = node;
            if (isBound) BoundCells.Add(node.Pos);
            else FloorCells.Add(node.Pos);
        }

    }
}