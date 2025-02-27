using static SystemEnum;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class BattleFieldSpawnInfo
{
    [SerializeField]
    public List<FieldSpawnInfo> fieldSpawnInfos = new();

    public Dictionary<eCharType, List<Vector3>> Convert2Dict()
    {
        Dictionary<eCharType, List<Vector3>> dict = new();
        foreach (var info in fieldSpawnInfos)
        {
            dict.Add(info.SpawnType, info.SpawnPositions);
        }
        return dict;
    }
}

[Serializable]
public class FieldSpawnInfo
{
    [SerializeField] private eCharType spawnType;
    [SerializeField] private List<Vector3> spawnPositions = new();

    public eCharType SpawnType => spawnType;
    public List<Vector3> SpawnPositions => spawnPositions;
    public int SpawnerCount => spawnPositions.Count;

    public FieldSpawnInfo(eCharType spawnType, List<Vector3> spawnList)
    {
        this.spawnType = spawnType;
        spawnPositions = spawnList;
    }
}