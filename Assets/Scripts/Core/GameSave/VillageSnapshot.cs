using Core.GameSave.Contracts;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Core.GameSave
{
    public class VillageSnapshot : FeatureSnapshot
    {
        public const int V = 1;
        public override int CurrentVersion => V;

        [JsonProperty] public string        Dungeon; // 던전 이름(또는 enum 문자열)
        [JsonProperty] public int           Floor; // 층 수
        [JsonProperty] public int           PlayerX; // 플레이어 X/Y
        [JsonProperty] public int           PlayerY;
        [JsonProperty] public List<int>     DiscoveredTiles = new();
        [JsonProperty] public List<int>     ItemIds = new();
        [JsonProperty] public int           Seed; // 재현용 시드

        public VillageSnapshot() : base("Village", V) { }

        public override void MigrateIfNeeded()
        {
            BumpVersion();
        }
    }
}