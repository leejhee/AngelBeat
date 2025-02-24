#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System;
using static SystemEnum;
using System.Linq;

public class SpawnPointEditor : EditorWindow
{
    private const string TARGET_SCENE_PATH = "Assets/Scenes/Editing/LevelEditingScene.unity";
    private const string PREF_SPAWN_INDICATOR = "PREF_SPAWN_INDICATOR";

    private SerializedObject targetObject;

    private StageMap targetStageMap;
    private GameObject targetInstance;

    private SpawnIndicator IndicatorPrefab;
    private eCharType _currentCharType = eCharType.None;
    private Dictionary<eCharType, Color> typeColorMapping = new();
    private Dictionary<eCharType, List<SpawnIndicator>> SpawnIndicators = new();

    private Vector2 scrollPosition; 

    [MenuItem("Tools/StageMap Editor")]
    public static void ShowWindow()
    {
        var window = GetWindow<SpawnPointEditor>();
        window.titleContent = new GUIContent("StageMap Editor");
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene(TARGET_SCENE_PATH);
        }
        else
        {
            window.Close();
        }
    }

    private void OnEnable()
    {
        targetObject = new SerializedObject(this);

        LoadSpawnIndicatorPrefab();
        LoadColorPreferences();

        EditorSceneManager.sceneOpened          += OnSceneOpened;
        SceneView.duringSceneGui                += ModalScene;
        SceneView.duringSceneGui                += DeploySpawner;
        EditorApplication.playModeStateChanged  += ModalEditingState;
        Undo.undoRedoPerformed                  += UndoRedoMap;
    }

    private void OnDisable()
    {
        SaveSpawnIndicatorPrefab();
        SaveColorPreferences();

        EditorSceneManager.sceneOpened          -= OnSceneOpened;
        SceneView.duringSceneGui                -= ModalScene;
        SceneView.duringSceneGui                -= DeploySpawner;
        EditorApplication.playModeStateChanged  -= ModalEditingState;
        Undo.undoRedoPerformed                  -= UndoRedoMap;


    }

    #region 에디팅 시 안전성 보장
    private void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        if(scene.path != TARGET_SCENE_PATH)
        {
            EditorApplication.delayCall += () =>
            {
                EditorSceneManager.OpenScene(TARGET_SCENE_PATH);
                Debug.LogError("안되지 안돼~ 하던거 다 하고 해라.");
            };
        }
    }

    private void ModalScene(SceneView view)
    {
        Event e = Event.current;

        if(e.type == EventType.ExecuteCommand &&
            (e.commandName == "NewScene" || e.commandName == "OpenScene"))
        {
            e.Use();
            ShowNotification(new GUIContent("씬 이동을 차단합니다."));
        }
    }

    // 그럴 일은 없겠지만 플레이도 못하게 한다. 플레이 할 거면 에디팅 안할 때 해!
    private void ModalEditingState(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            if (SceneManager.GetActiveScene().path == TARGET_SCENE_PATH)
            {
                EditorApplication.isPlaying = false;
                Debug.LogError("LevelEditingScene에서는 게임을 실행할 수 없습니다.");
            }
        }

        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            Debug.Log("게임 실행으로 인해 StageMap Editor 창이 자동으로 닫힙니다.");
            Close();
        }
    }

    [InitializeOnLoad]
    public static class EditorLockMonitor
    {
        static EditorLockMonitor()
        {
            EditorApplication.update += () =>
            {
                if (HasOpenInstances<SpawnPointEditor>() &&
                    SceneManager.GetActiveScene().path != TARGET_SCENE_PATH)
                {
                    EditorApplication.delayCall += () =>
                        EditorSceneManager.OpenScene(TARGET_SCENE_PATH);
                }
            };
        }
    }

    #endregion

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(position.width), GUILayout.Height(position.height));

        #region Select and Instantiate StageMap Component
        GUILayout.Label("레벨 디자인 맵 선택", EditorStyles.boldLabel);
        StageMap newStageMap = EditorGUILayout.ObjectField("Target StageMap", targetStageMap, typeof(StageMap), true) as StageMap;
        EditorGUILayout.HelpBox("StageMap 컴포넌트가 있는지 확인하세요.", MessageType.Warning);
        if (newStageMap != targetStageMap)
        {
            targetStageMap = newStageMap;
            LoadStageMapIntoScene();
        }
        IndicatorPrefab = (SpawnIndicator)EditorGUILayout.ObjectField("Spawn Indicator Prefab", IndicatorPrefab, typeof(SpawnIndicator), false);
        #endregion

        GUILayout.Space(10);

        #region Customize Prefab Color      
        GUILayout.Label("타입별 색상 설정", EditorStyles.boldLabel);
        foreach (var key in typeColorMapping.Keys.ToArray())
        {
            Color newColor = EditorGUILayout.ColorField($"{key} 색상", typeColorMapping[key]);

            // ✅ 색상이 변경된 경우, 모든 해당 타입의 SpawnIndicator 색상 업데이트
            if (typeColorMapping[key] != newColor)
            {
                typeColorMapping[key] = newColor;
                UpdateSpawnIndicatorColors(key, newColor);
            }
        }
        #endregion

        GUILayout.Space(10);

        #region Select Deploy Mode
        GUILayout.Label("유닛 배치 모드 선택", EditorStyles.boldLabel);

        string[] modeNames = Enum.GetValues(typeof(eCharType))
                                .Cast<eCharType>()
                                .Where(mode => mode != eCharType.eMax) // eMax 제외
                                .Select(mode => mode.ToString())       // 문자열 변환
                                .ToArray();

        _currentCharType = (eCharType)GUILayout.Toolbar((int)_currentCharType, modeNames);
        GUIStyle boldStyle = new(EditorStyles.label) { fontStyle = FontStyle.Bold };
        EditorGUILayout.LabelField("⚠️ 타 작업 전환 시 None 모드로 세팅하세요.", boldStyle);
        EditorGUILayout.HelpBox($"현재 선택된 배치 모드: {_currentCharType}", MessageType.Info);

        #endregion

        GUILayout.Space(10);

        #region Show SpawnIndicator List Info
        // 키를 제거하는 이유 : 메모리 허리띠 졸라매기를 하기 위해서
        foreach (var type in SpawnIndicators.Keys.ToList()) 
        {
            if (!SpawnIndicators.ContainsKey(type)) continue; 

            GUILayout.Label($"[ {type} ]", EditorStyles.boldLabel);

            for (int i = SpawnIndicators[type].Count - 1; i >= 0; i--)
            {
                if (SpawnIndicators[type][i] == null) continue;

                EditorGUILayout.BeginHorizontal();

                SpawnIndicators[type][i] = (SpawnIndicator)EditorGUILayout.ObjectField($"Indicator {i + 1}", SpawnIndicators[type][i], typeof(SpawnIndicator), true);

                Vector2 position = new(SpawnIndicators[type][i].transform.position.x, SpawnIndicators[type][i].transform.position.y);
                EditorGUILayout.LabelField($"위치: {position}", GUILayout.Width(120));
                EditorGUILayout.LabelField($"순서: {i + 1}", GUILayout.Width(50));

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

        #endregion

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Save Spawn Points"))
        {
            SaveSpawnPoints();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Close Editor"))
        {
            Close();
        }

        EditorGUILayout.EndScrollView();

    }

    #region 에디터 내 스테이지 로드
    public void LoadStageMapIntoScene()
    {
        if (targetStageMap == null) return;
        // 그럴일은 없겠지만? 체크 또 체크한다. 이 윈도우를 켰는데 씬이 만~약에 목표 씬이 아니라면 옮긴다.
        if (SceneManager.GetActiveScene().path != TARGET_SCENE_PATH)
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(TARGET_SCENE_PATH);
            }
            else
            {
                Debug.LogWarning("현재 씬이 저장되지 않아 씬 전환이 취소되었습니다.");
                return;
            }
        }

        targetInstance = Instantiate(targetStageMap.gameObject);
        targetInstance.name = "StageMap_Editor_Instance";

        if (targetInstance != null)
        {
            DestroyImmediate(targetInstance);
        }

        targetInstance = Instantiate(targetStageMap.gameObject);
        targetInstance.name = "StageMap_Editor_Instance";

        SceneManager.MoveGameObjectToScene(targetInstance, SceneManager.GetActiveScene());
        Undo.RegisterCreatedObjectUndo(targetInstance, "Load StageMap for Editing");

        Debug.Log("StageMap이 LevelEditingScene에 로드되었습니다.");
    }

    #endregion

    #region 에디터 내 스포너 배치 및 관리
    private void DeploySpawner(SceneView view)
    {
        if (_currentCharType == eCharType.None || _currentCharType == eCharType.eMax || IndicatorPrefab == null)
            return;

        Event e = Event.current;
        if (e.type == EventType.MouseDown && e.button == 0)
        {
            Vector3 worldPos = HandleUtility.GUIPointToWorldRay(e.mousePosition).origin;
            worldPos.z = 0;

            SpawnIndicator newIndicator = Instantiate(IndicatorPrefab, worldPos, Quaternion.identity);
            newIndicator.SetIndicator(_currentCharType, typeColorMapping[_currentCharType]);

            if (!SpawnIndicators.ContainsKey(_currentCharType))
            {
                SpawnIndicators[_currentCharType] = new List<SpawnIndicator>();
            }

            SpawnIndicators[_currentCharType].Add(newIndicator);

            Undo.RegisterCreatedObjectUndo(newIndicator.gameObject, "Create Spawn Indicator");
            Undo.undoRedoPerformed += UndoRedoIndicator;
            e.Use();
        }
    }

    private void ClearAllIndicators()
    {
        foreach (var type in SpawnIndicators.Keys.ToList())
        {
            foreach (var indicator in SpawnIndicators[type])
            {
                if (indicator != null) DestroyImmediate(indicator.gameObject);
            }
        }
        SpawnIndicators.Clear();
    }

    private void SaveSpawnPoints()
    {
        if (targetStageMap == null)
        {
            Debug.LogError("StageMap이 설정되지 않았습니다. 저장 불가.");
            return;
        }

        var newSpawnDict = new Dictionary<eCharType, List<Vector3>>();

        foreach (var type in SpawnIndicators.Keys)
        {
            newSpawnDict[type] = new List<Vector3>();

            foreach (var indicator in SpawnIndicators[type])
            {
                if (indicator == null) continue;
                newSpawnDict[type].Add(indicator.transform.position);
            }
        }

        Undo.RecordObject(targetStageMap, "Save Spawn Points");

        typeof(StageMap)
            .GetField("_spawnDict", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(targetStageMap, newSpawnDict);

        Debug.Log("Spawn Points가 StageMap에 저장되었습니다.");

        EditorUtility.SetDirty(targetStageMap);
    }
    #endregion

    #region 스포너 프리팹 저장 및 로드(굳이? 싶긴 한데 그냥 반복동작 방지)
    private void SaveSpawnIndicatorPrefab()
    {
        if (IndicatorPrefab != null)
        {
            string path = AssetDatabase.GetAssetPath(IndicatorPrefab);
            EditorPrefs.SetString(PREF_SPAWN_INDICATOR, path);
        }
        else
        {
            EditorPrefs.DeleteKey(PREF_SPAWN_INDICATOR);
        }
    }

    private void LoadSpawnIndicatorPrefab()
    {
        if (EditorPrefs.HasKey(PREF_SPAWN_INDICATOR))
        {
            string path = EditorPrefs.GetString(PREF_SPAWN_INDICATOR);
            IndicatorPrefab = AssetDatabase.LoadAssetAtPath<SpawnIndicator>(path);
        }
    }
    #endregion

    #region 스포너 색깔 관리
    private void UpdateSpawnIndicatorColors(eCharType type, Color newColor)
    {
        if (!SpawnIndicators.ContainsKey(type)) return;

        foreach (var indicator in SpawnIndicators[type])
        {
            if (indicator != null)
            {
                indicator.UpdateColor(newColor); 
            }
        }

        Repaint(); 
    }

    private void LoadColorPreferences()
    {
        foreach (eCharType type in Enum.GetValues(typeof(eCharType)))
        {
            if (type == eCharType.None || type == eCharType.eMax) continue;

            string colorKey = $"SpawnPointEditor_{type}_Color";

            if (EditorPrefs.HasKey($"{colorKey}_R"))
            {
                float r = EditorPrefs.GetFloat($"{colorKey}_R");
                float g = EditorPrefs.GetFloat($"{colorKey}_G");
                float b = EditorPrefs.GetFloat($"{colorKey}_B");
                float a = EditorPrefs.GetFloat($"{colorKey}_A");
                typeColorMapping[type] = new Color(r, g, b, a);
            }
            else
            {
                typeColorMapping[type] = type switch
                {
                    eCharType.Player => Color.blue,
                    eCharType.Enemy => Color.red,
                    _ => Color.white
                };
            }
        }
    }

    private void SaveColorPreferences()
    {
        foreach (var key in typeColorMapping.Keys)
        {
            string colorKey = $"SpawnPointEditor_{key}_Color";
            Color color = typeColorMapping[key];

            EditorPrefs.SetFloat($"{colorKey}_R", color.r);
            EditorPrefs.SetFloat($"{colorKey}_G", color.g);
            EditorPrefs.SetFloat($"{colorKey}_B", color.b);
            EditorPrefs.SetFloat($"{colorKey}_A", color.a);
        }
    }
    #endregion

    private void UndoRedoMap()
    {
        if (targetInstance == null)
        {
            Debug.Log("Undo 실행됨 → StageMap을 None으로 변경");
            targetStageMap = null;
            Repaint();
        }
    }

    private void UndoRedoIndicator()
    {
        foreach (var type in SpawnIndicators.Keys.ToList())
        {
            SpawnIndicators[type].RemoveAll(indicator => indicator == null);
            if (SpawnIndicators[type].Count == 0)
            {
                SpawnIndicators.Remove(type);
            }
        }
        Repaint();
    }
}

#endif
