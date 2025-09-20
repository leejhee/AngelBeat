﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GamePlay.Features.Battle.Scripts.BattleMap
{
    [CreateAssetMenu(fileName = "BattleFieldGroup", menuName = "ScriptableObjects/BattleFieldGroup")]
    public class BattleFieldGroup : ScriptableObject
    {
        [Serializable]
        public struct StageFieldEntry
        {
            public string stageName;
            public AssetReferenceT<GameObject> stageField;
        }
        public List<StageFieldEntry> stages = new();
        
        public AssetReferenceT<GameObject> GetStageRef(string stageName)
        {
            if (string.IsNullOrEmpty(stageName))
                return stages.Count > 0 ? stages[UnityEngine.Random.Range(0, stages.Count)].stageField : null;

            int i = stages.FindIndex(s => s.stageName == stageName);
            return i >= 0 ? stages[i].stageField : null;
        }
    }
}