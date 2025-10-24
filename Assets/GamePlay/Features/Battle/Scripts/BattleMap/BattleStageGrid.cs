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
        private Dictionary<CharBase, Vector2Int> _unitToCell;
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
            _unitToCell = new Dictionary<CharBase, Vector2Int>();
        }
        
        /// <summary>
        /// 캐릭터 초기화 시 사용. BattleCharManager에 모든 유닛이 등록된 후에 사용
        /// </summary>
        public void RebuildCharacterPositions()
        {
            _characters.Clear();
            _unitToCell.Clear();
            foreach (CharBase unit in BattleCharManager.Instance.GetBattleMembers())
            {
                Vector2Int cell = _stage.WorldToCell(unit.CharTransform.position);
                _characters[cell] = unit;
                _unitToCell[unit] = cell;
            }
        }
        #endregion
        
        #region Cell Point Validation
        public bool IsInBounds(Vector2Int cell) => _stage.InBounds(cell);
        public bool IsOccupied(Vector2Int cell) => _characters.ContainsKey(cell);
        public bool IsWalkable(Vector2Int cell) => _walkable.Contains(cell) && !_obstacles.Contains(cell) && !IsOccupied(cell);
        public bool IsPlatform(Vector2Int cell) => _walkable.Contains(cell);
        public bool IsMaskable(Vector2Int cell) => !IsInBounds(cell) || !IsPlatform(cell);
        
        #endregion
        
        #region StageField API - Cell Point
        
        public Vector2Int WorldToCell(Vector2 w) => _stage.WorldToCell(w);
        public Vector2 CellToWorldCenter(Vector2Int c) => _stage.CellToWorldCenter(c);

        
        #endregion
        
        #region Grid Helper
        
        public bool TryFindLandingFloor(Vector2Int from, out Vector2Int landingCell, out int fallFloors)
        {
            landingCell = default; fallFloors = 0;
            if (!IsInBounds(from)) return false;
            int y = from.y - 1; // from가 공중이라면 아래부터 탐색
            while (y >= 0){
                var c = new Vector2Int(from.x, y);
                if (IsPlatform(c)){ landingCell = c; fallFloors = from.y - y; return true; }
                y--;
            }
            return false; 
        }
        
        public CharBase GetUnitAt(Vector2Int cell) => _characters.GetValueOrDefault(cell);
        
        /// <summary>
        /// 등록자가 Cell을 점유하게 함
        /// </summary>
        /// <param name="cell">좌표 포인트</param>
        /// <param name="registrant">등록자</param>
        public bool OccupyCell(Vector2Int cell, CharBase registrant)
        {
            if (!IsInBounds(cell) || !IsWalkable(cell)) return false;
            if(_unitToCell.TryGetValue(registrant, out Vector2Int c)) _characters.Remove(c);
            _characters[cell] = registrant; _unitToCell[registrant] = cell;
            return true;
        }
        
        /// <summary>
        /// Cell의 점유 및 등록 해제
        /// </summary>
        /// <param name="unit">해제하는 유닛</param>
        public bool RemoveUnit(CharBase unit){
            if (!_unitToCell.Remove(unit, out Vector2Int c)) return false;
            _characters.Remove(c);
            return true;
        }
        
        /// <summary>
        /// 유닛의 위치 이동 관리. 오로지 위치 정보만 이동(구체 이동 관리 X)
        /// </summary>
        /// <param name="unit">이동할 유닛</param>
        /// <param name="to">목적지</param>
        /// <returns>결과</returns>
        public bool MoveUnit(CharBase unit, Vector2Int to)
        {
            if(!_unitToCell.TryGetValue(unit, out Vector2Int c)) return OccupyCell(to, unit);
            if (to != c && !IsWalkable(to)) return false;
            _characters.Remove(c);
            _characters[to] = unit; _unitToCell[unit] = to;
            return true;
        }

        #endregion
    }
}