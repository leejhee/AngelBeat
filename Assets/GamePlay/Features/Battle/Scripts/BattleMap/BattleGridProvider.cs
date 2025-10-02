using System;
using System.ComponentModel;
using UnityEngine;

namespace GamePlay.Features.Battle.Scripts.BattleMap
{
    public class BattleGridProvider : MonoBehaviour
    {
        private SpriteRenderer _sr;
        [SerializeField, ReadOnly(true)] private Vector2Int cellSize;

        private void Start()
        {
            _sr = GetComponent<SpriteRenderer>();
        }

        public void SetSelectionGridFrame(Vector2Int size)
        {
            cellSize = size;
            var mat = _sr.material;
            
        }
        
    }
}