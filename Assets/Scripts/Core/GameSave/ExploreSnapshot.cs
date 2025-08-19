using Newtonsoft.Json;
using Core.GameSave.Contracts;
using System;
using System.Collections.Generic;

namespace Core.GameSave
{
    [Serializable]
    public class ExploreSnapshot : FeatureSnapshot
    {
        public const int V = 1;
        public override int CurrentVersion => V;
        
        [JsonProperty] public string dungeon;           // 던전 이름(또는 enum 문자열)
        [JsonProperty] public int    floor;             // 층 수
        [JsonProperty] public int    playerX;           // 플레이어 X/Y
        [JsonProperty] public int    playerY;
        [JsonProperty] public List<int> discoveredTiles = new();
        [JsonProperty] public List<int> itemIds = new();
        [JsonProperty] public int    seed;              // 재현용 시드

        public ExploreSnapshot() : base("Explore", V) { }

        public override void MigrateIfNeeded()
        {
            BumpVersion();
        }
    }
}