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

        public void SelectSlot(int slotIndex)
        {
            selectedSlotIndex = slotIndex;
        }
        
        /// <summary>
        /// 세이브데이터에 마지막으로 플레이하고 저장됐던 슬롯을 플레이합니다.
        /// </summary>
        public void ContinueSlot()
        {
            
        }
        
        /// <summary>
        /// 현재 '지정된 슬롯'을 불러와서 플레이한다.
        /// </summary>
        public void PlaySlot(int slotIndex)
        {
            
        }

        public void NewGameStart()
        {
            
        }
        
    }
}