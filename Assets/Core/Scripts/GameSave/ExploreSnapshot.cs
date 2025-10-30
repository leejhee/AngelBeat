using Core.Scripts.Foundation.Define;
using Core.Scripts.GameSave.Contracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Scripts.GameSave
{
    [Serializable]
    public class ExploreSnapshot : FeatureSnapshot
    {
        public const int V = 1;
        public override int CurrentVersion => V;
        
        [JsonProperty] public SystemEnum.Dungeon                                currentDungeon;
        [JsonProperty] public int                                               currentFloor;
        [JsonProperty] public ulong                                             mapSeed;
        [JsonProperty] public Vector2Int                                        playerPosition;
        [JsonProperty] public List<int>                                         itemIds = new();
        [JsonProperty] public bool                                              isExploring;
        [JsonProperty] public DateTime                                          explorationStartTime;
        
        [JsonProperty] public bool[] visitedCells;
        [JsonProperty] public string[] completedEvents;
        

        public ExploreSnapshot() : base("Explore", V)
        {
            currentDungeon = SystemEnum.Dungeon.None;
            currentFloor = 0;
            playerPosition = Vector2Int.zero;
            mapSeed = 0;
            isExploring = false;
            explorationStartTime = DateTime.Now;
            visitedCells = new bool[0];
            completedEvents = new string[0];
        }
        
        public override void MigrateIfNeeded()
        {
            if (Version < CurrentVersion)
            {
                // 버전 업그레이드 로직
                Debug.Log($"[ExploreSnapshot] Migrating from version {Version} to {CurrentVersion}");
                BumpVersion();
            }
        }

        /// <summary>
        /// 새 탐사 시작
        /// </summary>
        public void StartNewExploration(SystemEnum.Dungeon dungeon, int floor, ulong seed, Vector2Int startPos, int mapSize)
        {
            currentDungeon = dungeon;
            currentFloor = floor;
            mapSeed = seed;
            playerPosition = startPos;
            isExploring = true;
            explorationStartTime = DateTime.Now;
            visitedCells = new bool[mapSize];
            completedEvents = new string[0];
        }

        /// <summary>
        /// 플레이어 위치 업데이트
        /// </summary>
        public void UpdatePlayerPosition(Vector2Int newPos, int cellIndex)
        {
            playerPosition = newPos;
            if (cellIndex >= 0 && cellIndex < visitedCells.Length)
            {
                visitedCells[cellIndex] = true;
            }
        }

        /// <summary>
        /// 탐사 완료
        /// </summary>
        public void CompleteExploration()
        {
            isExploring = false;
        }

        /// <summary>
        /// 특정 셀이 방문되었는지 확인
        /// </summary>
        public bool IsCellVisited(int cellIndex)
        {
            return cellIndex >= 0 && cellIndex < visitedCells.Length && visitedCells[cellIndex];
        }

        /// <summary>
        /// 이벤트 완료 추가
        /// </summary>
        public void AddCompletedEvent(string eventId)
        {
            var newEvents = new string[completedEvents.Length + 1];
            Array.Copy(completedEvents, newEvents, completedEvents.Length);
            newEvents[completedEvents.Length] = eventId;
            completedEvents = newEvents;
        }

        /// <summary>
        /// 이벤트가 완료되었는지 확인
        /// </summary>
        public bool IsEventCompleted(string eventId)
        {
            return Array.IndexOf(completedEvents, eventId) >= 0;
        }
    }
}