using Core.Scripts.Foundation.Define;
using GamePlay.Features.Explore.Scripts.Map.Data;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace GamePlay.Features.Explore.Scripts.Map.Logic
{
    /// <summary>
    /// MapRoot에 붙어있는 
    /// </summary>
    public class ExploreMap : MonoBehaviour
    {
        [SerializeField] private ExploreMapConfigDB configDB;
        [SerializeField] private ExploreSymbolDB symbolDB;
        [SerializeField] private ExploreEventDB eventDB;
        
        [Header("Map State")]
        [SerializeField] private ExploreMapSkeleton currentSkeleton;
        [SerializeField] private Transform tileParent;
        [SerializeField] private Transform symbolParent;
        
        // 타일 및 심볼 인스턴스들
        private Dictionary<Vector2Int, GameObject> tileInstances = new();
        private Dictionary<Vector2Int, GameObject> symbolInstances = new();
        
        // 생성된 맵 정보
        private ExploreMapConfig currentConfig;
        private SystemEnum.Dungeon currentDungeon;
        private int currentFloor;
        private ulong currentSeed;
        
        // Events
        public event System.Action<Vector2Int> OnTileClicked;
        
        private void Awake()
        {
            // 부모 오브젝트들 자동 생성
            if (tileParent == null)
            {
                var tileParentGO = new GameObject("Tiles");
                tileParentGO.transform.SetParent(transform);
                tileParent = tileParentGO.transform;
            }
            
            if (symbolParent == null)
            {
                var symbolParentGO = new GameObject("Symbols");
                symbolParentGO.transform.SetParent(transform);
                symbolParent = symbolParentGO.transform;
            }
        }
        
        
        /// <summary>
        /// 시스템에서 받아온 시드로부터 맵을 생성해주는 메서드
        /// </summary>
        public async UniTask GenerateMap(ulong seed, int floor, SystemEnum.Dungeon dungeon)
        {
            try
            {
                if (!configDB.TryGetConfig(dungeon, floor, out ExploreMapConfig config))
                {
                    Debug.LogError($"Invalid dungeon parameter - {dungeon} & {floor} ");
                    return;
                }
                
                currentConfig  = config;
                currentDungeon = dungeon;
                currentFloor   = floor;
                currentSeed    = seed;
                
                ClearExistingMap();

                // 3. 스켈레톤 생성
                currentSkeleton = await ExploreMapGenerator.BuildSkeleton(currentConfig, seed);
                
                // 4. 비주얼 생성
                await GenerateVisuals();
                
                Debug.Log($"[ExploreMap] Map generated successfully: {dungeon} Floor {floor}, Size: {currentSkeleton.Width}x{currentSkeleton.Height}");
                
#if UNITY_EDITOR
                LogMapInfo();
#endif
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ExploreMap] Failed to generate map: {ex}");
            }
            
        }
        
        /// <summary>
        /// 타일 프리팹을 인스턴스화하여 Skeleton을 구현
        /// </summary>
        private async UniTask GenerateVisuals()
        {
            if (currentSkeleton == null || currentConfig == null) return;

            await UniTask.SwitchToThreadPool();
            
            // 타일 생성 작업 준비
            var tileCreationTasks = new List<(Vector2Int pos, SystemEnum.MapCellType type)>();
            var symbolCreationTasks = new List<(Vector2Int pos, SkeletonSymbol symbol)>();
            
            // 모든 셀 순회
            for (int y = 0; y < currentSkeleton.Height; y++)
            {
                for (int x = 0; x < currentSkeleton.Width; x++)
                {
                    var pos = new Vector2Int(x, y);
                    var cellType = currentSkeleton.GetCellType(x, y);
                    
                    tileCreationTasks.Add((pos, cellType));
                    
                    // 심볼이 있다면 추가
                    var symbols = currentSkeleton.GetSymbolsAt(x, y).ToList();
                    foreach (var symbol in symbols)
                    {
                        symbolCreationTasks.Add((pos, symbol));
                    }
                }
            }
            
            await UniTask.SwitchToMainThread();
            
            // 타일 생성
            foreach (var (pos, cellType) in tileCreationTasks)
            {
                CreateTileAt(pos, cellType);
            }
            
            // 심볼 생성
            foreach (var (pos, symbol) in symbolCreationTasks)
            {
                CreateSymbolAt(pos, symbol);
            }
            
            // 카메라 위치 조정 (옵션)
            AdjustCameraPosition();
        }
        
        /// <summary>
        /// 특정 위치에 타일 생성
        /// </summary>
        private void CreateTileAt(Vector2Int position, SystemEnum.MapCellType cellType)
        {
            GameObject prefab = GetTilePrefab(cellType);
            if (prefab == null) return;
            
            var instance = Instantiate(prefab, tileParent);
            instance.transform.position = new Vector3(position.x, 0, position.y);
            instance.name = $"{cellType}_({position.x},{position.y})";
            
            // 클릭 이벤트 추가
            var clickHandler = instance.GetComponent<TileClickHandler>();
            if (clickHandler == null)
            {
                clickHandler = instance.AddComponent<TileClickHandler>();
            }
            clickHandler.Initialize(position, this);
            
            tileInstances[position] = instance;
        }
        
        /// <summary>
        /// 특정 위치에 심볼 생성
        /// </summary>
        private void CreateSymbolAt(Vector2Int position, SkeletonSymbol symbol)
        {
            GameObject prefab = GetSymbolPrefab(symbol.Type);
            if (prefab == null) return;
            
            var instance = Instantiate(prefab, symbolParent);
            instance.transform.position = new Vector3(position.x, 0.1f, position.y); // 타일보다 약간 위에
            instance.name = $"{symbol.Type}_({position.x},{position.y})";
            
            // 심볼별 추가 설정
            ConfigureSymbol(instance, symbol);
            
            symbolInstances[position] = instance;
        }
        
        /// <summary>
        /// 셀 타입에 맞는 타일 프리팹 반환
        /// </summary>
        private GameObject GetTilePrefab(SystemEnum.MapCellType cellType)
        {
            return cellType switch
            {
                SystemEnum.MapCellType.Floor => currentConfig.floorPrefab,
                SystemEnum.MapCellType.Wall => currentConfig.wallPrefab,
                _ => null
            };
        }
        
        /// <summary>
        /// 심볼 타입에 맞는 프리팹 반환
        /// </summary>
        private GameObject GetSymbolPrefab(SystemEnum.MapSymbolType symbolType)
        {
            if (symbolDB?.TryGet(symbolType, out GameObject prefab) == true)
            {
                return prefab;
            }
            return null;
        }
        
        /// <summary>
        /// 심볼별 특수 설정
        /// </summary>
        private void ConfigureSymbol(GameObject symbolInstance, SkeletonSymbol symbol)
        {
            // 심볼 타입별 특수 처리
            switch (symbol.Type)
            {
                case SystemEnum.MapSymbolType.Event:
                    if (symbol.EventType.HasValue)
                    {
                        var eventComponent = symbolInstance.GetComponent<EventSymbolBehaviour>();
                        if (eventComponent != null)
                        {
                            eventComponent.SetEventType(symbol.EventType.Value);
                        }
                    }
                    break;
                    
                case SystemEnum.MapSymbolType.Item:
                    if (symbol.ItemIndex.HasValue)
                    {
                        var itemComponent = symbolInstance.GetComponent<ItemSymbolBehaviour>();
                        if (itemComponent != null)
                        {
                            itemComponent.SetItemIndex(symbol.ItemIndex.Value);
                        }
                    }
                    break;
            }
        }
        
        /// <summary>
        /// 기존 맵 정리
        /// </summary>
        private void ClearExistingMap()
        {
            // 타일 인스턴스 정리
            foreach (var tile in tileInstances.Values)
            {
                if (tile != null) DestroyImmediate(tile);
            }
            tileInstances.Clear();
            
            // 심볼 인스턴스 정리
            foreach (var symbol in symbolInstances.Values)
            {
                if (symbol != null) DestroyImmediate(symbol);
            }
            symbolInstances.Clear();
            
            currentSkeleton = null;
        }
        
        /// <summary>
        /// 카메라 위치 조정
        /// </summary>
        private void AdjustCameraPosition()
        {
            if (currentSkeleton == null) return;
            
            var camera = Camera.main;
            if (camera == null) return;
            
            // 맵 중앙으로 카메라 이동
            float centerX = currentSkeleton.Width / 2f;
            float centerZ = currentSkeleton.Height / 2f;
            
            var cameraTransform = camera.transform;
            cameraTransform.position = new Vector3(centerX, cameraTransform.position.y, centerZ - 5f);
        }
        
        #region Public Interface
        
        /// <summary>
        /// 맵이 초기화되었는지 확인
        /// </summary>
        public bool IsInitialized => currentSkeleton != null;
        
        /// <summary>
        /// 맵 크기 반환 (셀 개수)
        /// </summary>
        public int GetMapSize()
        {
            return currentSkeleton?.Width * currentSkeleton?.Height ?? 0;
        }
        
        /// <summary>
        /// 좌표를 셀 인덱스로 변환
        /// </summary>
        public int GetCellIndex(Vector2Int position)
        {
            if (currentSkeleton == null || !currentSkeleton.InBounds(position.x, position.y))
                return -1;
            
            return currentSkeleton.ToIndex(position.x, position.y);
        }
        
        /// <summary>
        /// 유효한 위치인지 확인
        /// </summary>
        public bool IsValidPosition(Vector2Int position)
        {
            return currentSkeleton?.InBounds(position.x, position.y) ?? false;
        }
        
        /// <summary>
        /// 바닥 타일인지 확인
        /// </summary>
        public bool IsFloorTile(Vector2Int position)
        {
            return currentSkeleton?.IsFloor(position.x, position.y) ?? false;
        }
        
        /// <summary>
        /// StartPoint 심볼 위치 찾기
        /// </summary>
        public Vector2Int GetStartPosition()
        {
            if (currentSkeleton == null) return Vector2Int.zero;
            
            IEnumerable<SkeletonSymbol> startSymbols = currentSkeleton.GetSymbolsOfType(SystemEnum.MapSymbolType.StartPoint);
            SkeletonSymbol firstStart = startSymbols.FirstOrDefault();
            
            return firstStart != default ? new Vector2Int(firstStart.X, firstStart.Y) : Vector2Int.zero;
        }
        
        /// <summary>
        /// 타일 클릭 이벤트 발생
        /// </summary>
        public void NotifyTileClicked(Vector2Int position)
        {
            OnTileClicked?.Invoke(position);
        }
        
        #endregion
        
        #region Debug
        
        #if UNITY_EDITOR
        private void LogMapInfo()
        {
            if (currentSkeleton == null) return;
            
            var symbols = currentSkeleton.CollectSymbols();
            Debug.Log($"[ExploreMap] Generated map with {symbols.Count} symbols:");
            
            foreach (var symbolType in System.Enum.GetValues(typeof(SystemEnum.MapSymbolType)))
            {
                var count = symbols.Count( s => s.Type == (SystemEnum.MapSymbolType)symbolType);
                if (count > 0)
                {
                    Debug.Log($"  - {symbolType}: {count}");
                }
            }
        }
        
        [ContextMenu("Regenerate Current Map")]
        private async void RegenerateCurrentMap()
        {
            if (currentSeed != 0 && currentDungeon != SystemEnum.Dungeon.None)
            {
                await GenerateMap(currentSeed, currentFloor, currentDungeon);
            }
        }
        
        [ContextMenu("Clear Map")]
        private void ClearMap()
        {
            ClearExistingMap();
        }
        #endif
        
        #endregion
    }
    
    /// <summary>
    /// 타일 클릭 처리를 위한 간단한 컴포넌트
    /// </summary>
    public class TileClickHandler : MonoBehaviour
    {
        private Vector2Int position;
        private ExploreMap exploreMap;
        
        public void Initialize(Vector2Int pos, ExploreMap map)
        {
            position = pos;
            exploreMap = map;
        }
        
        private void OnMouseDown()
        {
            exploreMap?.NotifyTileClicked(position);
        }
    }
    
    /// <summary>
    /// 이벤트 심볼 동작 (예시)
    /// </summary>
    public class EventSymbolBehaviour : MonoBehaviour
    {
        public SystemEnum.CellEventType eventType;
        
        public void SetEventType(SystemEnum.CellEventType type)
        {
            eventType = type;
        }
    }
    
    /// <summary>
    /// 아이템 심볼 동작 (예시)
    /// </summary>
    public class ItemSymbolBehaviour : MonoBehaviour
    {
        public long itemIndex;
        
        public void SetItemIndex(long index)
        {
            itemIndex = index;
        }
    }
}
