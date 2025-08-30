﻿using Core.GameSave.Contracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Core.Scripts.GameSave
{
    [Serializable]
    public sealed class BattleSnapshot : FeatureSnapshot
    {
        public const int V = 1;
        public override int CurrentVersion => V;

        [JsonProperty] public string EncounterId;
        [JsonProperty] public List<int> PartyHp = new();
        [JsonProperty] public int    Turn;
        [JsonProperty] public bool   InBossPhase;

        public BattleSnapshot() : base("Battle", V) { }

        public override void MigrateIfNeeded()
        {
            BumpVersion();
        }
    }
}