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
        public int LastPlayedSlotIndex = -1;
        public readonly int maxSlotCount = 10; 
        
        [Header("전역 설정")]
        public GameSettings GameSettings;
        
        /// <summary>
        /// 
        /// </summary>
        public GlobalSaveData()
        {
            UID = Guid.NewGuid().ToString();
            FirstInstallTime = DateTime.Now.Ticks;
            
            GameSettings = new GameSettings();
            
        }
        
    }
}