﻿using Newtonsoft.Json;
using System;
using System.Text;
using UnityEngine;
using Core.Foundation.Utils;
using Core.GameSave.Contracts;
using System.Collections.Generic;
using System.Runtime.Serialization;
using static Core.Foundation.Define.SystemEnum;

namespace Core.GameSave
{
    /// <summary>
    /// 게임 실제 슬롯마다의 데이터
    /// </summary>
    [Serializable]
    public class GameSlotData
    {
        #region System Part
        [JsonProperty("firstCreationTime")] public DateTime firstCreationTime; 
        [JsonProperty("lastSavedTime")]     public DateTime lastSavedTime;
        [JsonProperty("lastGameState")]     public GameState lastGameState;
        [JsonProperty("playTime")]          public long playTimeTicks;
        [JsonProperty("slotName")]          public string slotName = "New Game";
        [JsonProperty("slotSeed")]          public ulong SlotSeed;
        [JsonProperty("RngCounters")]       public Dictionary<string, ulong> RngCounters;
        
        
        [JsonIgnore] 
        public TimeSpan PlayTime
        {
            get => new(playTimeTicks);
            set => playTimeTicks = value.Ticks;
        }
        /// <summary>
        /// 사용할 의사난수 객체
        /// </summary>
        [JsonIgnore] public RngHubStateless RNG;
        
        #endregion        
        
        #region Gameplay Part
        
        [JsonProperty("Features")]
        //반드시 이 구조로 간다.
        public Dictionary<string, FeatureSnapshot> Features;
        
        #endregion
        
        #region Initialization & Deserialization Event
        
        /// <summary>
        /// 세이브 매니저에서 생성할 시 호출되는 생성자
        /// </summary>
        public GameSlotData(string slotName)
        {
            firstCreationTime = DateTime.Now;
            lastSavedTime = DateTime.Now;
            Features ??= new();
            this.slotName = slotName;
        }
        
        /// <summary>
        /// 역직렬화 시 호출되는 생성자.
        /// </summary>
        [JsonConstructor]
        public GameSlotData(
            [JsonProperty("lastSavedTime")] DateTime lastSavedTime,
            [JsonProperty("lastGameState")] GameState lastGameState,
            [JsonProperty("playTime")] long playTimeTicks,
            [JsonProperty("slotName")] string slotName)
        {
            this.slotName = slotName;
            this.lastSavedTime = lastSavedTime;
            this.lastGameState = lastGameState;
            this.playTimeTicks =  playTimeTicks;
            Features ??= new();
            Debug.Log($"Game Slot Loaded : {slotName} (saved at {lastSavedTime})");
        }

        [OnDeserialized]
        internal void OnDeserialized(StreamingContext _)
        {
            RngCounters ??= new();
            if (SlotSeed == 0)
            {
                Debug.LogError($"[GameSlotData] SlotSeed is 0 for slot '{slotName}'. Seed must be derived at creation.");
                return;
            }

            RNG = new RngHubStateless(SlotSeed, RngCounters);
        }


        #endregion
        
        #region Read and Write
        
        /// <summary>
        /// feature을 key로 하여 덮어씌우는 식의 저장
        /// </summary>
        public void WriteSnapshot(FeatureSnapshot snapshot)
        {
            if (snapshot == null) return;
            Features ??= new();
            Features[snapshot.Feature] = snapshot;
        }
        
        public bool TryGet<T>(string featureKey, out T feature) where T : FeatureSnapshot
        {
            if (Features.TryGetValue(featureKey, out var v) && v is T t)
            {
                feature = t;
                return true;
            }
            feature = null;
            return false;
        }

        #endregion
        
        #region Util Part

        public string GetSlotSummary()
        {
            return new StringBuilder(slotName)
                .Append($"\nGameState : {lastGameState}")
                .Append($@"{PlayTime:hh\:mm\:ss}")
                .Append($"{lastSavedTime:yyyy/MM/dd HH:mm}")
                .ToString();
        }
        
        public void PrintAllStates()
        {
            Debug.Log($"=== GameSlot: {slotName} ===");
            Debug.Log($"Last Saved: {lastSavedTime}");
            Debug.Log($"Play Time: {PlayTime}");
            Debug.Log($"Last State: {lastGameState}");
        }
        
        #endregion
    }
}