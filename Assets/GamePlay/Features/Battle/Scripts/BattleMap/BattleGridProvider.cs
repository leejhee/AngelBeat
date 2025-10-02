using System;
using System.ComponentModel;
using UnityEngine;

namespace GamePlay.Features.Battle.Scripts.BattleMap
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class BattleGridProvider : MonoBehaviour
    {
        private SpriteRenderer _sr;
        private MaterialPropertyBlock _mpb;
        [SerializeField, ReadOnly(true)] private Vector2Int cellSize;

        private static readonly int ID_CELLS     = Shader.PropertyToID("_Cells");      
        private static readonly int ID_CELL_SIZE = Shader.PropertyToID("_CellSize");   
        private static readonly int ID_OFFSET    = Shader.PropertyToID("_Offset");     
        private static readonly int ID_THICKNESS = Shader.PropertyToID("_Thickness");
        
        private void Start()
        {
            _sr = GetComponent<SpriteRenderer>();
        }

        public void SetSelectionGridFrame(Vector2Int gridSize)
        {
            cellSize = gridSize;
            
            
        }
        
    }
}