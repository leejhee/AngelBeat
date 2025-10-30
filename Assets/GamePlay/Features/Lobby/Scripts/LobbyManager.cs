using Core.Scripts.Foundation.Define;
using Core.Scripts.Foundation.SceneUtil;
using Core.Scripts.GameSave;
using Core.Scripts.Managers;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay.Features.Lobby.Scripts
{
    public class LobbyManager : MonoBehaviour
    {
        private static LobbyManager instance;
        public static LobbyManager Instance
        {
            get
            {
                instance = FindObjectOfType<LobbyManager>();
                if(!instance)
                    instance = new GameObject("LobbyManager").AddComponent<LobbyManager>();

                return instance;
            }
        }
        
        [SerializeField] private string defaultSaveName = "새 여정 시작";
        private int selectedSlotIndex = -1;
        
        [Header("Debug 용도")]
        [SerializeField] private bool autoLoadLastSlot = false;
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            // SaveLoadManager 초기화 확인
            if (SaveLoadManager.Instance == null)
            {
                Debug.LogError("[LobbyManager] SaveLoadManager not initialized!");
                return;
            }

            // 디버그 모드: 마지막 플레이한 슬롯 자동 로드
            if (autoLoadLastSlot && SaveLoadManager.Instance.HasLastPlayed)
            {
                var lastSlotIndex = SaveLoadManager.Instance.GlobalSave.LastPlayedSlotIndex;
                Debug.Log($"[LobbyManager] Auto-loading last played slot: {lastSlotIndex}");
                ContinueGame(lastSlotIndex);
            }
        }
        
        /// <summary>
        /// 저장된 슬롯들을 뷰로 보여줍니다.
        /// </summary>
        private void ShowSlots()
        {
            List<SlotMetaData> list = SaveLoadManager.Instance.GetAllSlots();
            for (int i = 0; i < list.Count; i++)
            {
                var meta = list[i];
                Debug.Log($"[Slot {i}] {(meta.isEmpty ? "(빈 슬롯)" : meta.slotName)}");
            }
        }
        

        public void NewGameStart(string slotName)
        {
            if (string.IsNullOrWhiteSpace(slotName))
            {
                Debug.LogWarning("[LobbyManager] Slot name is empty!");
                // TODO: UI에 오류 표시
                return;
            }

            // 슬롯 생성
            bool success = SaveLoadManager.Instance.CreateNewSlot(slotName, out int slotIndex);
            
            if (!success)
            {
                Debug.LogError($"[LobbyManager] Failed to create new slot: {slotName}");
                // TODO: UI에 오류 표시 (슬롯 가득 참 등)
                return;
            }

            Debug.Log($"[LobbyManager] New slot created: {slotName} (Index: {slotIndex})");

            // 게임 시작 씬 - 튜토리얼로 전환 
            SceneLoader.LoadSceneWithLoading(SystemEnum.eScene.BattleTestScene);
        }
        
        /// <summary>
        /// 새 슬롯 생성 가능 여부 확인
        /// </summary>
        public bool CanCreateNewSlot()
        {
            var globalSave = SaveLoadManager.Instance.GlobalSave;
            return globalSave.HasEmptySlot;
        }

        /// <summary>
        /// 사용 가능한 슬롯 수 조회
        /// </summary>
        public int GetAvailableSlotCount()
        {
            var globalSave = SaveLoadManager.Instance.GlobalSave;
            return globalSave.maxSlotCount - globalSave.PlayableSlotCount;
        }
        
        public void ContinueGame(int slotIndex)
        {
            bool success = SaveLoadManager.Instance.LoadSlot(slotIndex);
            
            if (!success)
            {
                Debug.LogError($"[LobbyManager] Failed to load slot: {slotIndex}");
                // TODO: UI에 오류 표시
                return;
            }

            var currentSlot = SaveLoadManager.Instance.CurrentSlot;
            Debug.Log($"[LobbyManager] Slot loaded: {currentSlot.slotName}");
            Debug.Log($"[LobbyManager] Last game state: {currentSlot.lastGameState}");

            // 마지막 게임 상태에 따라 적절한 씬으로 전환
            LoadSceneByGameState(currentSlot.lastGameState);
        }

        /// <summary>
        /// 마지막 플레이한 슬롯으로 빠른 이어하기
        /// </summary>
        public void QuickContinue()
        {
            if (!SaveLoadManager.Instance.HasLastPlayed)
            {
                Debug.LogWarning("[LobbyManager] No last played slot found!");
                return;
            }

            var lastSlotIndex = SaveLoadManager.Instance.GlobalSave.LastPlayedSlotIndex;
            ContinueGame(lastSlotIndex);
        }
        
        #region 슬롯 관리
        
        /// <summary>
        /// 모든 슬롯 메타데이터 조회 (UI 표시용)
        /// </summary>
        public List<SlotMetaData> GetAllSlots()
        {
            return SaveLoadManager.Instance.GetAllSlots();
        }

        /// <summary>
        /// 슬롯 삭제
        /// </summary>
        public void DeleteSlot(int slotIndex)
        {
            bool success = SaveLoadManager.Instance.DeleteSlot(slotIndex);
            
            if (success)
            {
                Debug.Log($"[LobbyManager] Slot {slotIndex} deleted successfully.");
                // TODO: UI 갱신
            }
            else
            {
                Debug.LogError($"[LobbyManager] Failed to delete slot {slotIndex}");
            }
        }

        /// <summary>
        /// 특정 슬롯이 비어있는지 확인
        /// </summary>
        public bool IsSlotEmpty(int slotIndex)
        {
            var slots = GetAllSlots();
            if (slotIndex < 0 || slotIndex >= slots.Count)
                return true;
            
            return slots[slotIndex].isEmpty;
        }
        
        #endregion

        
        #region 씬 전환
        
        /// <summary>
        /// 게임 상태에 따라 적절한 씬으로 전환
        /// </summary>
        private void LoadSceneByGameState(SystemEnum.GameState gameState)
        {
            switch (gameState)
            {
                case SystemEnum.GameState.Village:
                    SceneLoader.LoadSceneWithLoading(SystemEnum.eScene.VillageScene);
                    break;
                
                case SystemEnum.GameState.Explore:
                    SceneLoader.LoadSceneWithLoading(SystemEnum.eScene.ExploreScene);
                    break;
                
                case SystemEnum.GameState.Battle:
                    SceneLoader.LoadSceneWithLoading(SystemEnum.eScene.BattleTestScene);
                    break;
                
                case SystemEnum.GameState.Lobby:
                    SceneLoader.LoadSceneWithLoading(SystemEnum.eScene.LobbyScene);
                    break;
                
                default:
                    SceneLoader.LoadSceneWithLoading(SystemEnum.eScene.LobbyScene);
                    break;
            }
        }
        #endregion
        
        /// <summary>
        /// 모든 슬롯 정보 출력 (디버깅용)
        /// </summary>
        [ContextMenu("Print All Slots")]
        public void PrintAllSlots()
        {
            var slots = GetAllSlots();
            Debug.Log($"=== All Slots ({slots.Count}) ===");
            
            for (int i = 0; i < slots.Count; i++)
            {
                SlotMetaData slot = slots[i];
                if (slot.isEmpty)
                {
                    Debug.Log($"Slot {i}: [EMPTY]");
                }
                else
                {
                    Debug.Log($"Slot {i}: {slot.slotName} | Last: {slot.lastGameState} | Time: {slot.playTimeTicks:hh\\:mm\\:ss}");
                }
            }
        }

        /// <summary>
        /// 글로벌 세이브 데이터 정보 출력
        /// </summary>
        [ContextMenu("Print Global Save Info")]
        public void PrintGlobalSaveInfo()
        {
            SaveLoadManager.Instance.GlobalSave.PrintDebugInfo();
        }

        /// <summary>
        /// 현재 슬롯의 RNG 카운터 정보 출력
        /// </summary>
        [ContextMenu("Print RNG Counters")]
        public void PrintRngCounters()
        {
            if (!SaveLoadManager.Instance.HasCurrentSlot)
            {
                Debug.LogWarning("[LobbyManager] No current slot loaded!");
                return;
            }

            var currentSlot = SaveLoadManager.Instance.CurrentSlot;
            Debug.Log($"=== RNG Counters for Slot: {currentSlot.slotName} ===");
            Debug.Log($"Slot Seed: {currentSlot.slotSeed}");
            Debug.Log($"Counters:");
            
            if (currentSlot.RngCounters != null && currentSlot.RngCounters.Count > 0)
            {
                foreach (var kvp in currentSlot.RngCounters)
                {
                    Debug.Log($"{kvp.Key}: {kvp.Value}");
                }
            }
            else
            {
                Debug.Log("(No counters yet)");
            }
        }
        
    }
}