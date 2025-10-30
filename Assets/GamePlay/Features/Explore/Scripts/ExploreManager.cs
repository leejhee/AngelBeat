using Core.Scripts.Foundation.Define;
using Core.Scripts.GameSave;
using Core.Scripts.GameSave.Contracts;
using Core.Scripts.Managers;
using Cysharp.Threading.Tasks;
using GamePlay.Common.Scripts.Entities.Character;
using GamePlay.Features.Explore.Scripts.Map.Data;
using GamePlay.Features.Explore.Scripts.Map.Logic;
using System;
using System.Net;
using UnityEngine;

namespace GamePlay.Features.Explore.Scripts
{
    /// <summary>
    /// 탐사 관리
    /// </summary>
    public class ExploreManager : MonoBehaviour, IFeatureSaveProvider
    {
        #region Singleton
        private static ExploreManager instance;
        public static ExploreManager Instance
        {
            get
            {
                instance = FindObjectOfType<ExploreManager>();
                if(!instance)
                    instance = new GameObject("ExploreManager").AddComponent<ExploreManager>();

                return instance;
            }
        }
        #endregion
        
        [Header("Map References")] 
        public ExploreMap exploreMap;
        
        [Header("Current Exploration State")]
        public SystemEnum.Dungeon currentDungeon;
        public int currentFloor;
        public Party playerParty;
        public Vector2Int playerPosition;
        public ulong currentMapSeed;
        
        public event Action<Vector2Int> OnPlayerMoved;
        public event Action<SystemEnum.Dungeon, int> OnExplorationStarted;
        public event Action OnExplorationCompleted;
        
        private ExploreSnapshot _currentSnapshot;
        private bool _isInitialized = false;
        
        #region Unity Events
        private void Awake()
        {
            if (!instance)
            {
                instance = this;
            }
            else if(instance != this)
            {
                Destroy(gameObject);
            }
        }

        
        private void OnEnable()
        {
            SaveLoadManager.Instance.RegisterProvider(this);
            SaveLoadManager.Instance.SlotLoaded += OnSlotLoaded;
        }

        private void OnDisable()
        {
            if (SaveLoadManager.Instance != null)
            {
                SaveLoadManager.Instance.SlotLoaded -= OnSlotLoaded;
                SaveLoadManager.Instance.UnregisterProvider(this);
            }
        }
        #endregion
        
        #region Initialization
        private async void Start()
        {
            // 탐사 초기화
            await ExploreInitialize();
        }

        private async UniTask ExploreInitialize()
        {
            try
            {
                // 1. ExploreMap 컴포넌트 찾기
                if (exploreMap == null)
                {
                    exploreMap = FindObjectOfType<ExploreMap>();
                    if (exploreMap == null)
                    {
                        Debug.LogError("[ExploreManager] ExploreMap component not found in scene!");
                        return;
                    }
                }

                // 2. 씬 전환 시 전달된 Payload 확인
                var payload = ExplorePayload.Instance;
                if (payload.TargetDungeon != SystemEnum.Dungeon.None)
                {
                    // 새 탐사 시작 또는 이어하기
                    await HandlePayloadExploration(payload);
                    payload.Clear(); // 사용 후 정리
                }
                else
                {
                    // 기존 세이브에서 복원
                    await HandleSavedExploration();
                }

                _isInitialized = true;
                Debug.Log($"[ExploreManager] Exploration initialized: {currentDungeon} Floor {currentFloor}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ExploreManager] Failed to initialize exploration: {ex}");
            }
        }
        
        
        /// <summary>
        /// Payload 기반 탐사 처리
        /// </summary>
        private async UniTask HandlePayloadExploration(ExplorePayload payload)
        {
            currentDungeon = payload.TargetDungeon;
            currentFloor = payload.TargetFloor;
            playerParty = payload.PlayerParty;

            if (payload.IsNewExplore)
            {
                // 새 탐사 시작
                await StartNewExploration();
            }
            else
            {
                // 기존 탐사 이어하기
                await ContinueExistingExploration();
            }
        }

        /// <summary>
        /// 저장된 탐사 상태 복원
        /// </summary>
        private async UniTask HandleSavedExploration()
        {
            if (SaveLoadManager.Instance.HasCurrentSlot &&
                SaveLoadManager.Instance.CurrentSlot.TryGet("Explore", out ExploreSnapshot snapshot))
            {
                await RestoreFromSnapshot(snapshot);
            }
            else
            {
                Debug.LogWarning("[ExploreManager] No saved exploration found. Starting default exploration.");
                // 기본값으로 시작 (개발/테스트용)
                currentDungeon = SystemEnum.Dungeon.MOUNTAIN_BACK;
                currentFloor = 1;
                await StartNewExploration();
            }
        }

        /// <summary>
        /// 새 탐사 시작
        /// </summary>
        private async UniTask StartNewExploration()
        {
            // 1. RNG에서 맵 시드 생성
            if (SaveLoadManager.Instance.HasCurrentSlot)
            {
                var rng = SaveLoadManager.Instance.CurrentSlot.RNG;
                currentMapSeed = rng.DeriveAndIncrementSeed($"Explore_{currentDungeon}_{currentFloor}");
            }
            else
            {
                currentMapSeed = (ulong)DateTime.Now.Ticks;
            }

            // 2. 맵 생성
            await exploreMap.GenerateMap(currentMapSeed, currentFloor, currentDungeon);

            // 3. 플레이어 시작 위치 설정 (StartPoint 심볼 위치)
            playerPosition = FindStartPosition();

            // 4. 탐사 스냅샷 초기화
            _currentSnapshot = new ExploreSnapshot();
            _currentSnapshot.StartNewExploration(
                currentDungeon, 
                currentFloor, 
                currentMapSeed, 
                playerPosition,
                exploreMap.GetMapSize()
            );

            // 5. 첫 셀 방문 처리
            int startCellIndex = exploreMap.GetCellIndex(playerPosition);
            _currentSnapshot.UpdatePlayerPosition(playerPosition, startCellIndex);

            OnExplorationStarted?.Invoke(currentDungeon, currentFloor);
            Debug.Log($"[ExploreManager] New exploration started at {currentDungeon} Floor {currentFloor}, Seed: {currentMapSeed}");
        }

        /// <summary>
        /// 기존 탐사 이어하기
        /// </summary>
        private async UniTask ContinueExistingExploration()
        {
            if (SaveLoadManager.Instance.HasCurrentSlot &&
                SaveLoadManager.Instance.CurrentSlot.TryGet("Explore", out ExploreSnapshot snapshot))
            {
                await RestoreFromSnapshot(snapshot);
            }
            else
            {
                Debug.LogWarning("[ExploreManager] No existing exploration to continue. Starting new one.");
                await StartNewExploration();
            }
        }

        /// <summary>
        /// 스냅샷에서 탐사 상태 복원
        /// </summary>
        private async UniTask RestoreFromSnapshot(ExploreSnapshot snapshot)
        {
            _currentSnapshot = snapshot;
            currentDungeon = snapshot.currentDungeon;
            currentFloor = snapshot.currentFloor;
            currentMapSeed = snapshot.mapSeed;
            playerPosition = snapshot.playerPosition;

            // 동일한 시드로 맵 재생성
            await exploreMap.GenerateMap(currentMapSeed, currentFloor, currentDungeon);

            Debug.Log($"[ExploreManager] Exploration restored: {currentDungeon} Floor {currentFloor}");
        }

        /// <summary>
        /// 시작 위치 찾기 (StartPoint 심볼 위치)
        /// </summary>
        private Vector2Int FindStartPosition()
        {
            // ExploreMap에서 StartPoint 위치를 가져오는 로직
            // 임시로 (0,0) 반환
            return Vector2Int.zero;
        }

        #endregion

        #region Save/Load Events
        
        private void OnSlotLoaded(GameSlotData slotData)
        {
            if (slotData.TryGet("Explore", out ExploreSnapshot snapshot))
            {
                RebuildExploreState(snapshot);
            }
        }
        
        private void RebuildExploreState(ExploreSnapshot snapshot)
        {
            if (!_isInitialized) return;

            _currentSnapshot = snapshot;
            currentDungeon = snapshot.currentDungeon;
            currentFloor = snapshot.currentFloor;
            currentMapSeed = snapshot.mapSeed;
            playerPosition = snapshot.playerPosition;

            Debug.Log($"[ExploreManager] Exploration state rebuilt from save");
        }
        
        #endregion

        #region Player Movement
        
        /// <summary>
        /// 플레이어를 특정 위치로 이동
        /// </summary>
        public void MovePlayerTo(Vector2Int targetPosition)
        {
            if (!_isInitialized || _currentSnapshot == null) return;

            // 이동 가능 여부 검증
            if (!CanMoveTo(targetPosition))
            {
                Debug.LogWarning($"[ExploreManager] Cannot move to {targetPosition}");
                return;
            }

            // 위치 업데이트
            playerPosition = targetPosition;
            int cellIndex = exploreMap.GetCellIndex(targetPosition);
            _currentSnapshot.UpdatePlayerPosition(targetPosition, cellIndex);

            OnPlayerMoved?.Invoke(playerPosition);
            
            // 자동 저장
            SaveLoadManager.Instance.SaveSlotByCurrentState();
            
            Debug.Log($"[ExploreManager] Player moved to {targetPosition}");
        }

        /// <summary>
        /// 해당 위치로 이동 가능한지 확인
        /// </summary>
        private bool CanMoveTo(Vector2Int position)
        {
            return exploreMap.IsValidPosition(position) && exploreMap.IsFloorTile(position);
        }

        /// <summary>
        /// 타일 클릭 시 최단거리 이동
        /// </summary>
        public void OnTileClicked(Vector2Int clickedPosition)
        {
            if (!CanMoveTo(clickedPosition)) return;

            // TODO: 최단거리 경로 찾기 및 순차 이동
            // 일단은 직접 이동
            MovePlayerTo(clickedPosition);
        }

        #endregion

        #region Public Properties
        
        public bool IsExploring => _currentSnapshot?.isExploring ?? false;
        public bool IsInitialized => _isInitialized;
        
        /// <summary>
        /// 현재 맵에서 특정 셀이 방문되었는지 확인
        /// </summary>
        public bool IsCellVisited(Vector2Int position)
        {
            if (_currentSnapshot == null) return false;
            int cellIndex = exploreMap.GetCellIndex(position);
            return _currentSnapshot.IsCellVisited(cellIndex);
        }

        /// <summary>
        /// 탐사 완료 처리
        /// </summary>
        public void CompleteExploration()
        {
            if (_currentSnapshot != null)
            {
                _currentSnapshot.CompleteExploration();
                OnExplorationCompleted?.Invoke();
                
                // 최종 저장
                SaveLoadManager.Instance.SaveSlotByCurrentState();
                
                Debug.Log("[ExploreManager] Exploration completed!");
            }
        }
        
        #endregion
        
        #region IFeatureSaveProvider Members
        public string FeatureName => "Explore";
        
        public FeatureSnapshot Capture()
        {
            return _currentSnapshot ?? new ExploreSnapshot();
        }

        #endregion
    }
}