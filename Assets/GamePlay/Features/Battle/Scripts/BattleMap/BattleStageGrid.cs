using GamePlay.Features.Battle.Scripts.Unit;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GamePlay.Features.Battle.Scripts.BattleMap
{
    [RequireComponent(typeof(StageField))]
    public class BattleStageGrid : MonoBehaviour
    {
        private StageField _stage;

        private Dictionary<Vector2Int, CharBase> _characters;
        private HashSet<Vector2Int> _walkable;
        private HashSet<Vector2Int> _obstacles;
        
        
        #region Public Properties
        /// <summary> 여기서 필요한 cell position 가져다 쓸 것 </summary>
        public IReadOnlyDictionary<Vector2Int, CharBase> CharacterPositions => _characters;
        public IReadOnlyCollection<Vector2Int> WalkablePositions => _walkable;
        public IReadOnlyCollection<Vector2Int> ObstaclePositions => _obstacles;
        #endregion
        
        #region Initialization
        public void InitGrid(StageField staticField)
        {
            _stage = staticField;
            _walkable = staticField.PlatformGridCells.ToHashSet();
            _obstacles = staticField.ObstacleGridCells.ToHashSet();
            _characters = new Dictionary<Vector2Int, CharBase>();
        }
        
        /// <summary>
        /// 캐릭터 초기화 시 사용. BattleCharManager에 모든 유닛이 등록된 후에 사용
        /// </summary>
        public void RebuildCharacterPositions()
        {
            _characters.Clear();
            foreach (CharBase unit in BattleCharManager.Instance.GetBattleMembers())
            {
                Vector2Int cell = _stage.WorldToCell(unit.CharTransform.position);
                _characters[cell] = unit;
            }
        }
        #endregion
        
        #region Cell Point Validation
        public bool IsInBounds(Vector2Int cell) => _stage.InBounds(cell);
        public bool IsOccupied(Vector2Int cell) => _characters.ContainsKey(cell);
        public bool IsWalkable(Vector2Int cell) => _walkable.Contains(cell) && !_obstacles.Contains(cell) && !IsOccupied(cell);
        
        #endregion
        
        #region StageField API - Cell Point
        
        public Vector2Int WorldToCell(Vector2 w) => _stage.WorldToCell(w);
        public Vector2 CellToWorldCenter(Vector2Int c) => _stage.CellToWorldCenter(c);
        
        #endregion
        
    }
}