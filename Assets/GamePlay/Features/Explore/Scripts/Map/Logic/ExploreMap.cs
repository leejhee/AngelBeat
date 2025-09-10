using GamePlay.Explore.Map.Logic;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay.Features.Scripts.Explore.Map.Logic
{
    /// <summary>
    /// 맵의 기본 구조를 나타내는 클래스
    /// </summary>
    [Serializable]
    public class ExploreMap
    {
        private Dictionary<Vector2Int, ExploreMapNode> _tileMap = new();
        

    }
}