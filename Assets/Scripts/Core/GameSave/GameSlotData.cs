using GamePlay.Battle.Save;
using GamePlay.Character.Save;
using GamePlay.Explore;
using GamePlay.Village;
using Newtonsoft.Json;
using System;
using System.Text;
using UnityEngine;
using Utils.Core.Random;
using static SystemEnum;

namespace Core.GameSave
{
    /// <summary>
    /// 게임 실제 슬롯마다의 데이터
    /// </summary>
    [Serializable]
    public class GameSlotData
    {
        #region System Part
        [JsonProperty("lastSavedTime")] public DateTime lastSavedTime;
        [JsonProperty("lastGameState")] public GameState lastGameState;
        [JsonProperty("playTime")]      public long playTimeTicks;
        [JsonProperty("slotName")] public string slotName = "New Game";
        
        [JsonIgnore]
        public TimeSpan PlayTime
        {
            get => new(playTimeTicks);
            set => playTimeTicks = value.Ticks;
        }
        
        #endregion        
        
        #region Gameplay Part

        [JsonProperty("characterProgressData")]
        public CharacterProgressSaveData characterProgress;
        
        [JsonProperty("partyData")] 
        public ExploreSaveData exploreData;

        [JsonProperty("villageData")] 
        public VillageSaveData villageData;
        
        [JsonProperty("battleData")] 
        public BattleSaveData battleData;
        
        [JsonIgnore] public GameRandom gameRnd;
        
        #endregion
        
        #region Cosntructor
        public GameSlotData()
        {
            // 새 슬롯 생성 시의 시간에 따라 난수기 결정. 
            gameRnd = new GameRandom((ulong)lastSavedTime.Ticks);
        }

        public GameSlotData(string slotName)
        {
            gameRnd = new GameRandom((ulong)lastSavedTime.Ticks);
            this.slotName = slotName;
        }
        
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
            
            Debug.Log($"Game Slot Loaded : {slotName} (saved at {lastSavedTime})");
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