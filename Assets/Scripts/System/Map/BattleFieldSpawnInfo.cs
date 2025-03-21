using static SystemEnum;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class BattleFieldSpawnInfo
{
    [SerializeField]
    public List<FieldSpawnInfo> fieldSpawnInfos = new();

    [SerializeField]
    public List<FieldObjectInfo> fieldObjectInfos = new();

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

/// <summary> 유닛 스폰 정보 </summary>
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

[Serializable]
public class FieldObjectInfo
{
    [SerializeField] private string prefabName;
    [SerializeField] private Vector3 position;

    public string PrefabName => prefabName;
    public Vector3 Position => position;

    public FieldObjectInfo(string prefabName, Vector3 position)
    {
        this.prefabName = prefabName;
        this.position = position;
    }
}