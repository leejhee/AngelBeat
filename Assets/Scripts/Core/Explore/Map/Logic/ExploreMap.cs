﻿using Core.GameSave;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Explore.Map.Logic
{
    /// <summary>
    /// 맵의 기본 구조를 나타내는 클래스
    /// </summary>
    [Serializable]
    public class ExploreMap : ISavableEntity
    {
        private string _randomSeed;
        private Dictionary<Vector2Int, ExploreMapNode> _tileMap = new();
        
        
        
        #region ISavable Implementation
        public void Save()
        {
            throw new NotImplementedException();
        }

        public void Load()
        {
            throw new NotImplementedException();
        }

        public bool IsDirty()
        {
            throw new NotImplementedException();
        }

        public void ClearDirty()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}