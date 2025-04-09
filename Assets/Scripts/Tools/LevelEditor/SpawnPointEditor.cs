#if UNITY_EDITOR
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static SystemEnum;


public class SpawnPointEditor
{
    private SpawnIndicator SpawnIndicatorPrefab { get; set; }
    private Dictionary<eCharType, Color> TypeColorMapping { get; set; }
    private Dictionary<eCharType, List<SpawnIndicator>> SpawnIndicators { get; set; }
    private eCharType CurrentCharType { get; set; } = eCharType.None;

    private Transform _indicatorRoot;
    private Transform IndicatorRoot
    {
        get
        {
            if (!_indicatorRoot)
            {
                GameObject go = GameObject.Find("IndicatorRoot");
                if (!go)
                    go = new GameObject("IndicatorRoot");
                _indicatorRoot = go.transform;
            }
            return _indicatorRoot;
        }
    }

    public SpawnPointEditor(SpawnIndicator prefab)
    {
        SpawnIndicatorPrefab = prefab;
        TypeColorMapping = new Dictionary<eCharType, Color>();
        SpawnIndicators = new Dictionary<eCharType, List<SpawnIndicator>>();
    }

    #region 인디케이터 색깔 관리(저장, 로드, 업데이트)
    public void LoadColorPreferences()
    {
        foreach (eCharType type in Enum.GetValues(typeof(eCharType)))
        {
            if (type == eCharType.None || type == eCharType.eMax)
                continue;
            string key = $"StageEditor_{type}_Color";
            if (EditorPrefs.HasKey($"{key}_R"))
            {
                float r = EditorPrefs.GetFloat($"{key}_R");
                float g = EditorPrefs.GetFloat($"{key}_G");
                float b = EditorPrefs.GetFloat($"{key}_B");
                float a = EditorPrefs.GetFloat($"{key}_A");
                TypeColorMapping[type] = new Color(r, g, b, a);
            }
            else
            {
                TypeColorMapping[type] = type == eCharType.Player ? Color.blue :
                                         type == eCharType.Enemy ? Color.red : Color.white;
            }
        }
    }

    public void SaveColorPreferences()
    {
        foreach (var kv in TypeColorMapping)
        {
            string key = $"StageEditor_{kv.Key}_Color";
            Color color = kv.Value;
            EditorPrefs.SetFloat($"{key}_R", color.r);
            EditorPrefs.SetFloat($"{key}_G", color.g);
            EditorPrefs.SetFloat($"{key}_B", color.b);
            EditorPrefs.SetFloat($"{key}_A", color.a);
        }
    }

    public void UpdateSpawnIndicatorColors(eCharType type, Color newColor)
    {
        if (!SpawnIndicators.ContainsKey(type)) return;
        foreach (var indicator in SpawnIndicators[type])
        {
            if (indicator != null)
                indicator.UpdateColor(newColor);
        }
    }

    #endregion

    
    /// <summary> Spawn Point 관련 OnGUI </summary>
    public void DrawGUI()
    {
        EditorGUILayout.LabelField("스폰 포인트 배치 모드", EditorStyles.boldLabel);

        SpawnIndicatorPrefab = (SpawnIndicator)EditorGUILayout.ObjectField("Spawn Indicator Prefab", SpawnIndicatorPrefab, typeof(SpawnIndicator), false);

        if (SpawnIndicatorPrefab == false)
        {
            EditorGUILayout.HelpBox("처음 여셨다면 SpawnIndicator을 할당해 주세요.", MessageType.Error); 
            return;
        }

        // 색상 커스터마이징
        EditorGUILayout.LabelField("타입별 색상 설정", EditorStyles.boldLabel);
        foreach (var key in TypeColorMapping.Keys.ToList())
        {
            Color newColor = EditorGUILayout.ColorField($"{key} 색상", TypeColorMapping[key]);
            if (TypeColorMapping[key] != newColor)
            {
                TypeColorMapping[key] = newColor;
                UpdateSpawnIndicatorColors(key, newColor);
            }
        }
        GUILayout.Space(10);

        // 배치 모드 선택
        EditorGUILayout.LabelField("유닛 배치 모드 선택", EditorStyles.boldLabel);
        string[] modeNames = Enum.GetValues(typeof(eCharType))
                                 .Cast<eCharType>()
                                 .Where(mode => mode != eCharType.eMax)
                                 .Select(mode => mode.ToString())
                                 .ToArray();
        CurrentCharType = (eCharType)GUILayout.Toolbar((int)CurrentCharType, modeNames);
        EditorGUILayout.HelpBox($"현재 선택된 배치 모드: {CurrentCharType}", MessageType.Info);
        GUILayout.Space(10);

        // 현재 배치된 SpawnIndicator 리스트 표시
        foreach (var type in SpawnIndicators.Keys.ToList())
        {
            EditorGUILayout.LabelField($"[ {type} ]", EditorStyles.boldLabel);
            for (int i = SpawnIndicators[type].Count - 1; i >= 0; i--)
            {
                if (!SpawnIndicators[type][i]) continue;
                EditorGUILayout.BeginHorizontal();
                SpawnIndicators[type][i] = (SpawnIndicator)EditorGUILayout.ObjectField(
                    $"Indicator {i + 1}", SpawnIndicators[type][i], typeof(SpawnIndicator), true);
                Vector2 pos = new Vector2(SpawnIndicators[type][i].transform.position.x,
                                          SpawnIndicators[type][i].transform.position.y);
                EditorGUILayout.LabelField($"위치: {pos}", GUILayout.Width(120));
                EditorGUILayout.LabelField($"순서: {i + 1}", GUILayout.Width(50));
                SpawnIndicators[type][i].spawnFixedIndex = EditorGUILayout.LongField("정해진 유닛 인덱스", SpawnIndicators[type][i].spawnFixedIndex);
                
                if (GUILayout.Button("Remove", GUILayout.Width(70)))
                {
                    Undo.DestroyObjectImmediate(SpawnIndicators[type][i].gameObject);
                    SpawnIndicators[type].RemoveAt(i);
                    if (SpawnIndicators[type].Count == 0)
                    {
                        SpawnIndicators.Remove(type);
                        EditorGUILayout.EndHorizontal();
                        break;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        if (GUILayout.Button("Clear All"))
        {
            ClearAllIndicators();
        }
    }

    /// <summary> Spawn Point 배치를 위한 씬 GUI 이벤트를 처리 </summary>
    public void OnSceneGUI(SceneView sceneView, StageField targetInstance)
    {
        if (CurrentCharType == eCharType.None || CurrentCharType == eCharType.eMax || SpawnIndicatorPrefab == null)
            return;
        Event e = Event.current;
        if (e.type == EventType.MouseDown && e.button == 0)
        {
            Vector3 worldPos = HandleUtility.GUIPointToWorldRay(e.mousePosition).origin;
            worldPos.z = 0;
            Vector3 localPos = targetInstance.transform.InverseTransformPoint(worldPos);

            SpawnIndicator newIndicator = UnityEngine.Object.Instantiate
                (SpawnIndicatorPrefab, localPos, Quaternion.identity, IndicatorRoot);
            newIndicator.SetIndicator(CurrentCharType, TypeColorMapping[CurrentCharType], 0);

            if (!SpawnIndicators.ContainsKey(CurrentCharType))
                SpawnIndicators[CurrentCharType] = new List<SpawnIndicator>();
            SpawnIndicators[CurrentCharType].Add(newIndicator);
            Undo.RegisterCreatedObjectUndo(newIndicator.gameObject, "Create Spawn Indicator");
            e.Use();
        }
    }

    public void ClearAllIndicators()
    {
        foreach (var type in SpawnIndicators.Keys.ToList())
        {
            foreach (var indicator in SpawnIndicators[type])
            {
                if (indicator)
                    UnityEngine.Object.DestroyImmediate(indicator.gameObject);
            }
        }
        SpawnIndicators.Clear();
    }

    public void RestoreSpawnIndicators(StageField targetInstance)
    {
        ClearAllIndicators();
        BattleFieldSpawnInfo spawnerData = targetInstance.LoadSpawnerOnlyInEditor();
        foreach (var info in spawnerData.fieldSpawnInfos)
        {
            if (!SpawnIndicators.ContainsKey(info.SpawnType))
                SpawnIndicators[info.SpawnType] = new List<SpawnIndicator>();
            foreach (var data in info.UnitSpawnList)
            {
                SpawnIndicator newIndicator = UnityEngine.Object.Instantiate
                    (SpawnIndicatorPrefab, 
                    targetInstance.transform.TransformPoint(data.SpawnPosition), 
                    Quaternion.identity, 
                    IndicatorRoot);
                newIndicator.SetIndicator(
                    info.SpawnType, 
                    TypeColorMapping[info.SpawnType], 
                    data.SpawnCharacterIndex);
                SpawnIndicators[info.SpawnType].Add(newIndicator);
            }
        }
        Debug.Log("이전 스폰 데이터를 기반으로 인디케이터를 복원했습니다.");
    }

    public void SaveSpawnPoints(StageField targetInstance)
    {
        if (!targetInstance)
        {
            Debug.LogError("StageMap이 설정되지 않았습니다. 저장 불가.");
            return;
        }
        Undo.RecordObject(targetInstance, "Save Spawn Points");
        SerializedObject so = new(targetInstance);
        SerializedProperty spawnerProp = so.FindProperty("battleSpawnerData");
        if (spawnerProp == null)
        {
            Debug.LogError("battleSpawnerData를 찾을 수 없습니다.");
            return;
        }
        BattleFieldSpawnInfo spawnerInfos = spawnerProp.managedReferenceValue as BattleFieldSpawnInfo;
        spawnerInfos ??= new BattleFieldSpawnInfo();

        spawnerInfos.fieldSpawnInfos.Clear();
        foreach (var kv in SpawnIndicators)
        {
            List<SpawnData> spawnDataList = new();
            foreach (var indicator in kv.Value)
            {
                spawnDataList.Add(new SpawnData(indicator.spawnFixedIndex, indicator.transform.position));
            }
            spawnerInfos.fieldSpawnInfos.Add(new FieldSpawnInfo(kv.Key, spawnDataList));
        }
        spawnerProp.managedReferenceValue = spawnerInfos;
        so.ApplyModifiedProperties();
        Debug.Log("Spawn Points가 StageMap에 저장되었습니다.");
        EditorUtility.SetDirty(targetInstance);
    }

    public void CleanupIndicators()
    {
        foreach (var type in SpawnIndicators.Keys.ToList())
        {
            SpawnIndicators[type].RemoveAll(ind => ind == null);
            if (SpawnIndicators[type].Count == 0)
                SpawnIndicators.Remove(type);
        }
    }
}

#endif