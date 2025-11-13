using Core.Attributes;
using Core.Scripts.Data;
using Core.Scripts.Foundation.Define;
using Core.Scripts.Managers;
using Cysharp.Threading.Tasks;
using GamePlay.Common.Scripts.Entities.Character;
using GamePlay.Features.Battle.Scripts.Unit;
using System;
using System.Collections.Generic;
using UIs.Runtime;
using UnityEngine;
using static Core.Scripts.Foundation.Define.SystemEnum;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GamePlay.Features.Battle.Scripts.BattleMap
{
    /// <summary>
    /// 전투 스테이지의 런타임용 데이터를 가지며,
    /// 전투 시스템 초기화 시 생성되는 스테이지 인스턴스
    /// </summary>
    [RequireComponent(typeof(Grid)), Serializable]
    public class StageField : MonoBehaviour
    {
        #region In-Editor Field
        private Dictionary<eCharType, List<SpawnData>> _spawnDict = new();
        
        [SerializeReference/*, CustomDisable*/] // 데이터 클래스에서 바로 파싱할 수 있도록 그냥 큰 단위 하나를 만듬
        private BattleFieldSpawnInfo battleSpawnerData = new();
        #endregion
        
        #region Runtime Providing Fields (편집 후 불변.)

        [SerializeField] private GameObject backgroundImage;
        
        /// <summary>
        /// 현재 맵에 존재하는 좌표의 총 사이즈(GridProvider의 초기화에 사용)
        /// </summary>
        [SerializeField] private Vector2Int gridSize;
        
        /// <summary>
        /// 미리보기 전용 그리드 제공자
        /// </summary>
        [SerializeField] private BattleGridProvider gridProvider;

        private Grid _grid;
        private Vector2 _cellWorld;
        private Vector2 _originWorld;

        [SerializeField] private List<Vector2Int> platformGridCells;
        [SerializeField] private List<ObstacleEntry> obstacleGridCells;
        [SerializeField] private List<CoverEntry> coverageGridCells;
        public Grid Grid => _grid;
        public Vector2Int GridSize => gridSize;
        public Vector2 CellWorld => _cellWorld;
        public Vector2 OriginWorld => _originWorld;
        
        public List<Vector2Int> PlatformGridCells => platformGridCells;
        public List<ObstacleEntry> ObstacleGridCells => obstacleGridCells;
        public List<CoverEntry> CoverageGridCells => coverageGridCells;

        public GameObject BackgroundImageSprite => backgroundImage;

        public Transform ObjectRoot { 
            get
            {
                GameObject root = GameObject.Find("ObjectRoot");
                if(root == null)
                {
                    root = new GameObject("ObjectRoot");
                    root.transform.SetParent(transform);
                }
                return root.transform;
            } 
        }
        #endregion
        
        #region Initialization
        
        private void Awake()
        {
            Debug.Log("Initializing Spawn Data...");
            _spawnDict = battleSpawnerData.Convert2Dict();
            _grid = GetComponent<Grid>();
            if(!gridProvider)
                gridProvider = GetComponentInChildren<BattleGridProvider>();
        }

        private void Start()
        {
            _cellWorld = new Vector2(
                _grid.cellSize.x * transform.lossyScale.x,
                _grid.cellSize.y * transform.lossyScale.y
            );
            
            ComputeGridBasis(out _cellWorld, out _originWorld, out _);
            gridProvider.ApplySpec(gridSize, _cellWorld, _originWorld, lineWidthPixels: 2);
            gridProvider.InitMask();
            gridProvider.Show(false);
            
            UIManager.Instance.InstantiateBackgroundObject(BackgroundImageSprite);
        }

        private void ComputeGridBasis(
            out Vector2 cellWorld,
            out Vector2 originWorld,
            out Vector2 sizeWorld)
        {
            cellWorld = new Vector2(
                _grid.cellSize.x * transform.lossyScale.x,
                _grid.cellSize.y * transform.lossyScale.y
            );
            
            Vector2Int halfGridSize = gridSize / 2;
            originWorld = (Vector2)_grid.GetCellCenterWorld(Vector3Int.zero) - 
                              new Vector2(halfGridSize.x + 0.5f, halfGridSize.y + 0.5f) * cellWorld;
            
            sizeWorld = new Vector2(gridSize.x * cellWorld.x,  gridSize.y * cellWorld.y);
        }
        
        #endregion

        #region Spawning Units for Initialization
        public void SpawnUnit(CharBase charBase, int squadOrder)
        {
            eCharType type = charBase.GetCharType();
            if (squadOrder >= _spawnDict[type].Count)
            {
                Debug.LogError("현재 편성 인원 기준을 맵이 수용하지 못합니다. 데이터 체크 요망");
                return;
            }
            BattleCharManager.Instance.CharGenerate(new CharParameter()
            {
                Scene = eScene.BattleTestScene,
                GeneratePos = _spawnDict[type][squadOrder].SpawnPosition,
                CharIndex = charBase.Index
            });
        }
        
        public void SpawnAllUnitsByType(eCharType type, List<CharBase> characters)
        {
            if (characters.Count > _spawnDict[type].Count)
            {
                Debug.LogError("현재 편성 인원 기준을 맵이 수용하지 못합니다. 데이터 체크 요망");
                return;
            }
            for (int i = 0; i < characters.Count; i++)
            {
                BattleCharManager.Instance.CharGenerate(new CharParameter()
                {
                    Scene = eScene.BattleTestScene,
                    GeneratePos = _spawnDict[type][i].SpawnPosition,
                    CharIndex = characters[i].Index
                });
            }
        }
        
        /// <summary>
        /// 스테이지에서 플레이어 파티와 적 캐릭터들을 인스턴스화하고, 그 전체 목록을 반환.
        /// </summary>
        /// <param name="playerParty"> 플레이어 측의 파티 </param>
        /// <returns> 스폰된 모든 캐릭터를 반환합니다. </returns>
        public async UniTask<List<CharBase>> SpawnAllUnits(Party playerParty)
        {
            Debug.Log("Spawning All Units...");
            List<CharBase> battleMembers = new();
            
            //PlayerSide
            List<CharacterModel> partyInfo = playerParty.partyMembers;
            for (int i = 0; i < partyInfo.Count; i++)
            {
                CharacterModel character = partyInfo[i];
                SpawnData data = _spawnDict[eCharType.Player][i];
                CharBase battlePrefab =
                    await BattleCharManager.Instance.CharGenerate(new CharBattleParameter(character, data.SpawnPosition));
                battleMembers.Add(battlePrefab);
            }

            //EnemySide
            foreach (var spawnData in _spawnDict[eCharType.Enemy])
            {
                long idx = spawnData.SpawnCharacterIndex;
                MonsterData data = DataManager.Instance.GetData<MonsterData>(idx);
                CharacterModel model = new(data);
                CharBase battlePrefab =
                    await BattleCharManager.Instance.CharGenerate(new CharBattleParameter(model, spawnData.SpawnPosition));
                battleMembers.Add(battlePrefab);
            }

            return battleMembers;
        }
        
        #endregion
        
        #region Grid Provider Util
        
        public void ShowGridOverlay(bool on)
        {
            gridProvider.Show(on);
            if (!on) { gridProvider.ClearHighlights(); gridProvider.ClearHover(); }
        }

    // 범위 칠하기(“가능 중 불가”만 빨강으로)
        public void PaintRange(IEnumerable<Vector2Int> possible, IEnumerable<Vector2Int> blocked, Vector2Int? selected = null)
        {
            gridProvider.SetHighlights(possible, blocked, selected);
        }

    // 호버 업데이트(좌표 변환 포함 예시)
        public void UpdateHoverFromWorld(Vector2 worldPos)
        {
            var cell = WorldToCell(worldPos);
            if (InBounds(cell)) gridProvider.SetHoverCell(cell);
            else                gridProvider.ClearHover();
        }

    // 좌표 유틸
        public bool InBounds(Vector2Int c) =>
            c.x >= 0 && c.y >= 0 && c.x < gridSize.x && c.y < gridSize.y;

        public Vector2Int WorldToCell(Vector2 world)
        {
            var p = new Vector2((world.x - _originWorld.x) / _cellWorld.x,
                (world.y - _originWorld.y) / _cellWorld.y);
            return new Vector2Int(Mathf.FloorToInt(p.x), Mathf.FloorToInt(p.y));
        }

        public Vector2 CellToWorldCenter(Vector2Int cell) =>
            _originWorld + (Vector2)(cell + Vector2.one * 0.5f) * _cellWorld;
        #endregion
        
        
        #region 에디터 툴용
    #if UNITY_EDITOR
        
        public BattleFieldSpawnInfo LoadSpawnerOnlyInEditor()
        {
            return battleSpawnerData;
        }

        private void OnDrawGizmosSelected()
        {
            _grid = GetComponent<Grid>();
            if (!_grid) return;
            
            ComputeGridBasis(out Vector2 cellW, out Vector2 originW, out Vector2 sizeW);
            
            // Grid 컴포넌트의 원점
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_grid.GetCellCenterWorld(Vector3Int.zero), Mathf.Min(cellW.x, cellW.y) * 0.1f);
            
            // 박스
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(originW + 0.5f * sizeW, sizeW);
            
            // 원점
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(originW, Mathf.Min(cellW.x, cellW.y) * 0.1f);

            var pad = 0.95f;
            Gizmos.color = new Color(0, 1, 0, 0.25f);
            foreach (var c in platformGridCells)
            {
                var p = originW + (c + Vector2.one * 0.5f) * cellW;
                Gizmos.DrawCube(p, cellW * pad);
            }
            Gizmos.color = new Color(1, 0, 0, 0.25f);
            foreach (var c in obstacleGridCells)
            {
                var p = originW + (c.cell + Vector2.one * 0.5f) * cellW;
                Gizmos.DrawCube(p, cellW * pad);
            }

            List<FieldSpawnInfo> fieldInfo = battleSpawnerData.fieldSpawnInfos;
            foreach (FieldSpawnInfo tp in fieldInfo)
            {
                if (tp.SpawnType == eCharType.Player)
                    Gizmos.color = new Color(1, 0.5f, 0);
                else if (tp.SpawnType == eCharType.Enemy)
                    Gizmos.color = new Color(0, 0, 1);
                foreach (var info in tp.UnitSpawnList)
                {
                    Gizmos.DrawSphere(info.SpawnPosition, 1f);
                }
                
            }
        }

        [ContextMenu("좌표 셀 유효성 검사(플랫폼 및 장애물)")]
        private void ValidateGridCells()
        {
            bool ok = true;
            ok &= ValidateList(platformGridCells, "platformGridCells");
            //ok &= ValidateList(obstacleGridCells, "obstacleGridCells");
            if (ok) Debug.Log("[StageField] Grid cell lists are valid.");
        }
        
        bool ValidateList(List<Vector2Int> list, string listName)
        {
            var set = new HashSet<Vector2Int>();
            bool ok = true;
            foreach (var c in list)
            {
                if (!(c is { x: >= 0, y: >= 0 } && c.x < gridSize.x && c.y < gridSize.y))
                {
                    Debug.LogWarning($"[StageField] {listName}: out of bounds {c} (grid {gridSize}).");
                    ok = false;
                }
                if (!set.Add(c))
                {
                    Debug.LogWarning($"[StageField] {listName}: duplicated cell {c}.");
                    ok = false;
                }
            }
            return ok;
        }
        
    #endif
        #endregion


    }





}
