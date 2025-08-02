using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.GameSave
{
    [Serializable]
    public class GlobalSaveData
    {
        [Header("영구 데이터")]
        public readonly string UID;
        public readonly long FirstInstallTime;
        
        [Header("슬롯 데이터")]
        public List<GameSlotData> GameSlots;
        public int LastPlayedSlotIndex;
        public readonly int maxSlotCount = 10; 
        
        [Header("전역 설정")]
        public GameSettings GameSettings;

        /// <summary>
        /// 마지막으로 플레이한 게임 데이터
        /// </summary>
        public GameSlotData LastPlayedSlotData
        {
            get
            {
                if (LastPlayedSlotIndex == -1 || LastPlayedSlotIndex >= GameSlots.Count)
                {
                    Debug.Log("No Slot Data Yet");
                    return null;
                }
                return GameSlots[LastPlayedSlotIndex];
            }
        }

        /// <summary>
        /// 어플리케이션 인스턴스 단위 전역 세이브 데이터
        /// </summary>
        public GlobalSaveData()
        {
            UID = Guid.NewGuid().ToString();
            FirstInstallTime = DateTime.Now.Ticks;
            
            GameSlots = new List<GameSlotData>();
            GameSettings = new GameSettings();

            LastPlayedSlotIndex = -1;
            maxSlotCount = 10;
        }
        
        
    }
}