using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Utils.Core.Random;
using static SystemEnum;

namespace Core.GameSave
{
    //NewtonSoft 쓰니까 전부 serialize 가능
    [Serializable]
    public class GameSlotData
    {
        #region System Part
        [JsonProperty("lastSavedTime")]
        public DateTime lastSavedTime;
        
        [JsonProperty("lastGameState")]
        public GameState lastGameState;

        [JsonProperty("playTime")] 
        public long playTimeTicks;

        [JsonIgnore]
        public TimeSpan PlayTime
        {
            get => new(playTimeTicks);
            set => playTimeTicks = value.Ticks;
        }
        
        [JsonProperty("slotName")]
        public string slotName = "New Game";
        
        [JsonProperty("stateSave")]
        public Dictionary<GameState, JObject> savedDict = new();
        
        #endregion        
        
        
        #region Skill Locker Part
        //이건 추후 ISavableEntity를 상속하는 애한테 갈 파트이다.
        public SystemEnum.eSkillUnlock skillLocker;
        public void SetSkillFlag(SystemEnum.eSkillUnlock changingLocker, bool flag)
        {
            if (flag)
            {
                skillLocker |= changingLocker;
            }
            else
            {
                skillLocker &= ~changingLocker;
            }
        }
        public bool GetSkillFlag(SystemEnum.eSkillUnlock target) => skillLocker.HasFlag(target);
        #endregion
        
        #region General Fiels
        [JsonIgnore]
        public GameRandom gameRnd;
        
        #endregion
        
        #region Cosntructor
        public GameSlotData()
        {
            gameRnd = new GameRandom((ulong)lastSavedTime.Ticks);
        }
        
        [JsonConstructor]
        public GameSlotData(
            [JsonProperty("lastSavedTime")] DateTime lastSavedTime,
            [JsonProperty("lastGameState")] GameState lastGameState,
            [JsonProperty("playTime")] long playTimeTicks,
            [JsonProperty("slotName")] string slotName,
            [JsonProperty("savedDict")] Dictionary<GameState, JObject> savedDict = null)
        {
            this.slotName = slotName;
            this.lastSavedTime = lastSavedTime;
            this.lastGameState = lastGameState;
            this.playTimeTicks =  playTimeTicks;
            this.savedDict = savedDict ?? new();
            
            Debug.Log($"Game Slot Loaded : {slotName} (saved at {lastSavedTime})");
        }
        
        #endregion
        
        #region Core
        public T GetStateData<T>(GameState state) where T : ISavableEntity, new()
        {
            if (savedDict.TryGetValue(state, out JObject obj))
            {
                try
                {
                    return JsonConvert.DeserializeObject<T>(obj.ToString());
                }
                catch (JsonException ex)
                {
                    Debug.LogError("[Deserialization Failed] : 안됐지롱. \n" +
                                   $"타입명 : {typeof(T).Name} | 저장 파트 : {state}\n" +
                                   $"{ex.Message}\n" +
                                   $"{ex.StackTrace}");
                }
            }
            T defaultState = new();
            SetStateData(state, defaultState);
            return defaultState;
        }

        public void SetStateData<T>(GameState state, T data) where T : ISavableEntity
        {
            if (data == null)
            {
                Debug.LogWarning($"자네 지금 null을 저장하려는 건가?");
                return;
            }

            try
            {
                savedDict[state] = JObject.FromObject(data);
                lastSavedTime = DateTime.Now;
                Debug.Log($"{state}의 데이터인 {typeof(T).Name}이 성공적으로 저장되었습니다.");
            }
            catch (JsonException ex)
            {
                Debug.LogError($"[Serialization Failed] : {state}의 데이터인 {typeof(T).Name} 저장 실패");
            }
        }

        public bool HasGameState(GameState state)
        {
            return savedDict.ContainsKey(state);
        }

        public void Clear()
        {
            savedDict.Clear();
            Debug.Log("이 슬롯의 모든 저장 상태 초기화");
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

        public List<GameState> GetSavedStates()
        {
            return new List<GameState>(savedDict.Keys);
        }
        
        public void PrintAllStates()
        {
            Debug.Log($"=== GameSlot: {slotName} ===");
            Debug.Log($"Last Saved: {lastSavedTime}");
            Debug.Log($"Play Time: {PlayTime}");
            Debug.Log($"Last State: {lastGameState}");
            Debug.Log($"Saved States: {string.Join(", ", savedDict.Keys)}");
        }
        
        #endregion
    }
}