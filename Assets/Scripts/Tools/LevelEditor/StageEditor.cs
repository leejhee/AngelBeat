#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

/*
public class StageEditor : EditorWindow{
    public enum ePlacementMode
    {
        Spawner,
        Object  
    }

    private const string TARGET_SCENE_PATH = "Assets/Scenes/Editing/LevelEditingScene.unity";
    private const string PREF_SPAWN_INDICATOR = "PREF_SPAWN_INDICATOR";

    /// <summary> 현재 편집할 맵의 프리팹이다. </summary>
    /// <remarks> <b>저장 및 로드 용도로만 사용함에 주의</b> </remarks>
    private StageField targetStageMap;

    /// <summary> 현재 올라가 있는 프리팹의 인스턴스이다. </summary>
    /// <remarks> <b>이거로만 내부에서 편집 작업한다!</b> </remarks>
    private StageField targetInstance;
   
    /// <summary> 배치 모드. 무엇을 배치할 건가요? </summary>
    private ePlacementMode _currentPlacementMode    = ePlacementMode.Spawner;
    
    #region Spawner Placing Mode

    /// <summary> 스포너 표지기 프리팹.</summary>
    /// <remarks> <b>처음에는 등록해주셔야 함</b> </remarks>
    private SpawnIndicator IndicatorPrefab;

    /// <summary> 씬의 스포너 표지기들이 모이는 곳. 깔끔한 하이어라키를 위함 </summary>
    private Transform IndicatorRoot
    {
        get
        {
            GameObject go = GameObject.Find("IndicatorRoot");
            if (go == null)
            {
                go = new GameObject("IndicatorRoot");
            }
            return go.transform;
        }
    }

    private eCharType _currentCharType              = eCharType.None;
    private Dictionary<eCharType, Color> typeColorMapping = new();
    private Dictionary<eCharType, List<SpawnIndicator>> SpawnIndicators = new();

    #endregion

    #region Object Placing Mode
    private GameObject _selectedPrefab;
    private SerializedProperty _prefabListProperty;

    [SerializeField]
    private List<GameObject> _prefabList = new();

    private List<FieldObjectInfo> _placedObjects = new();
    #endregion

    private Vector2 scrollPosition;
    private SerializedObject targetObject;

    [MenuItem("Tools/StageMap Editor")]
    public static void ShowWindow()
    {
        var window = GetWindow<StageEditor>();
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
        _prefabListProperty = targetObject.FindProperty("_prefabList");

        LoadSpawnIndicatorPrefab();
        LoadColorPreferences();

        LoadPrefabList();

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

        SavePrefabList();

        EditorSceneManager.sceneOpened          -= OnSceneOpened;
        SceneView.duringSceneGui                -= ModalScene;
        SceneView.duringSceneGui                -= DeploySpawner;
        EditorApplication.playModeStateChanged  -= ModalEditingState;
        Undo.undoRedoPerformed                  -= UndoRedoMap;


    }

   

    private void OnGUI()
    {
        #region Init Part
        _currentPlacementMode = (ePlacementMode)GUILayout.Toolbar((int)_currentPlacementMode,
        new string[] { "Spawner Mode", "Object Mode" });
       
        bool isSpawnerMode = (_currentPlacementMode == ePlacementMode.Spawner);
        bool isObjectMode = (_currentPlacementMode == ePlacementMode.Object);

        EditorGUILayout.HelpBox("맵 에디팅 완료 후 저장 시 반드시 Save Map을 누르고 닫아주세요. ", MessageType.Warning);
        GUILayout.Space(20);

        #endregion

        // 스크롤 뷰 시작
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);     

        #region Select and Instantiate StageMap Component
        GUILayout.Label("레벨 디자인 맵 선택", EditorStyles.boldLabel);
        StageField newStageMap = EditorGUILayout.ObjectField("Target StageMap", targetStageMap, typeof(StageField), true) as StageField;
        EditorGUILayout.HelpBox("StageMap 컴포넌트가 있는지 확인하세요.", MessageType.Warning);
        if (newStageMap != targetStageMap)
        {
            targetStageMap = newStageMap;
            LoadStageMapIntoScene();
        }
        IndicatorPrefab = (SpawnIndicator)EditorGUILayout.ObjectField("Spawn Indicator Prefab", IndicatorPrefab, typeof(SpawnIndicator), false);
        #endregion
        
        GUILayout.Space(10);

        #region SPAWNER PLACING MODE
        GUI.enabled = isSpawnerMode;

        #region Customize Prefab Color      
        GUILayout.Label("타입별 색상 설정", EditorStyles.boldLabel);
        foreach (var key in typeColorMapping.Keys.ToArray())
        {
            Color newColor = EditorGUILayout.ColorField($"{key} 색상", typeColorMapping[key]);

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

        if (GUILayout.Button("Save Spawn Points Only"))
        {
            SaveSpawnPoints();
        }
        #endregion

        GUI.enabled = true;
        #endregion

        #region OBJECT PAINTING MODE
        GUI.enabled = isObjectMode;

        #region Show Object Prefab List
        DrawObjectModeUI();
        #endregion

        GUI.enabled = true;
        #endregion

        EditorGUILayout.EndScrollView();

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Save Map"))
        {
            SavePrefab();
        }

        if (GUILayout.Button("Close Editor"))
        {
            TryCloseEditor();
        }
    }

    #region 에디팅 시 안전성 보장
    private void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        if (scene.path != TARGET_SCENE_PATH)
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

        if (e.type == EventType.ExecuteCommand &&
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
                if (HasOpenInstances<StageEditor>() &&
                    SceneManager.GetActiveScene().path != TARGET_SCENE_PATH)
                {
                    EditorApplication.delayCall += () =>
                        EditorSceneManager.OpenScene(TARGET_SCENE_PATH);
                }
            };
        }
    }

    #endregion

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

        if (targetInstance != null)
        {
            DestroyImmediate(targetInstance);
        }

        var targetGO = PrefabUtility.InstantiatePrefab(targetStageMap.gameObject) as GameObject;
        targetInstance = targetGO.GetComponent<StageField>();
        targetInstance.gameObject.name = "StageMap_Editor_Instance";

        SceneManager.MoveGameObjectToScene(targetInstance.gameObject, SceneManager.GetActiveScene());
        Undo.RegisterCreatedObjectUndo(targetInstance.gameObject, "Load StageMap for Editing");

        Debug.Log("StageMap이 LevelEditingScene에 로드되었습니다.");

        RestoreSpawnIndicators();
    }

    private void RestoreSpawnIndicators()
    {
        if (targetInstance == null) return;

        ClearAllIndicators();

        BattleFieldSpawnInfo spawnerData = targetInstance.LoadSpawnerOnlyInEditor();
        foreach (var info in spawnerData.fieldSpawnInfos)
        {
            if (!SpawnIndicators.ContainsKey(info.SpawnType))
            {
                SpawnIndicators[info.SpawnType] = new List<SpawnIndicator>();
            }

            foreach (var position in info.SpawnPositions)
            {
                SpawnIndicator newIndicator = Instantiate(
                    IndicatorPrefab,
                    targetInstance.transform.TransformPoint(position),
                    Quaternion.identity,
                    IndicatorRoot
                );

                newIndicator.SetIndicator(info.SpawnType, typeColorMapping[info.SpawnType]);
                SpawnIndicators[info.SpawnType].Add(newIndicator);
            }
        }

        Debug.Log("이전 스폰 데이터를 기반으로 인디케이터를 복원했습니다.");
    }


    #endregion

    #region 에디터 내 스포너 배치 및 관리

    /// <summary> SceneView에서 마우스로 클릭시, 모드를 감지하여 스포너를 배치 </summary>
    /// <param name="view"></param>
    private void DeploySpawner(SceneView view)
    {
        if (_currentCharType == eCharType.None || 
            _currentCharType == eCharType.eMax || 
            IndicatorPrefab == null)
            return;

        Event e = Event.current;
        if (e.type == EventType.MouseDown && e.button == 0)
        {
            Vector3 worldPos = HandleUtility.GUIPointToWorldRay(e.mousePosition).origin;
            worldPos.z = 0;
            Vector3 localPos = targetInstance.transform.InverseTransformPoint(worldPos);

            SpawnIndicator newIndicator = 
                Instantiate(IndicatorPrefab, localPos, Quaternion.identity, IndicatorRoot);
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
        if (targetInstance == null)
        {
            Debug.LogError("StageMap이 설정되지 않았습니다. 저장 불가.");
            return;
        }

        Undo.RecordObject(targetInstance, "Save Spawn Points");

        SerializedObject serializedObject = new(targetInstance);
        SerializedProperty infoProperty = serializedObject.FindProperty("battleSpawnerData");

        // 잘못 찾았다면 여기 걸린다. 여기서는 FindProperty 이름 확인하도록 한다.
        if (infoProperty == null)
        {
            Debug.LogError("battleSpawnerData를 찾을 수 없습니다.");
            return;
        }

        if (infoProperty.managedReferenceValue is not BattleFieldSpawnInfo spawnerinfos) 
            spawnerinfos = new BattleFieldSpawnInfo();

        spawnerinfos.fieldSpawnInfos.Clear(); //현재 씬에 풀어진 스포너 다 집어넣기 위해 캐시 클리어 해준다.

        foreach (var item in SpawnIndicators)
        {
            List<Vector3> newlist = new();
            foreach (var vectors in item.Value)
            {
                newlist.Add(vectors.transform.position);
            }
            spawnerinfos.fieldSpawnInfos.Add(new FieldSpawnInfo(item.Key, newlist));
        }

        infoProperty.managedReferenceValue = spawnerinfos;
        serializedObject.ApplyModifiedProperties();

        Debug.Log("Spawn Points가 StageMap에 저장되었습니다.");

        EditorUtility.SetDirty(targetInstance);
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

    private void DrawObjectModeUI()
    {
        GUILayout.Label("오브젝트 배치 모드", EditorStyles.boldLabel);

        targetObject.Update();

        EditorGUILayout.PropertyField(_prefabListProperty, new GUIContent(_prefabListProperty.displayName));

        for (int i = 0; i < _prefabList.Count; i++)
        {
            if (_prefabList[i] == null) continue;
        
            if (GUILayout.Button(_prefabList[i].name))
            {
                _selectedPrefab = _prefabList[i];
            }
        
            if (_selectedPrefab == _prefabList[i])
            {
                GUILayout.Space(5);
                Texture2D previewTexture = AssetPreview.GetAssetPreview(_selectedPrefab);
                if (previewTexture != null)
                {
                    GUILayout.Label(previewTexture, GUILayout.Width(100), GUILayout.Height(100));
                }
            }
        
        
        }

        targetObject.ApplyModifiedProperties();

        if (_selectedPrefab != null)
        {
            GUILayout.Label("현재 선택된 프리팹: " + _selectedPrefab.name);
        }
        else
        {
            GUILayout.Label("현재 선택된 프리팹이 없습니다.");
        }

        GUILayout.Space(10);

        if (!_isPainting && _selectedPrefab != null)
        {
            if (GUILayout.Button("Start Painting", GUILayout.Height(30)))
            {
                StartPainting();
            }
        }
        else if (_isPainting)
        {
            if (GUILayout.Button("Stop Painting", GUILayout.Height(30)))
            {
                StopPainting();
            }
        }

    }

    bool _isPainting;
    private GameObject _previewObject; 

    private void StartPainting()
    {
        if (_selectedPrefab == null) return;

        _isPainting = true;

        SceneView.duringSceneGui += OnSceneGUI;

        if (_previewObject == null)
        {
            _previewObject = Instantiate(_selectedPrefab);
            _previewObject.name = "PreviewObject";
            _previewObject.hideFlags = HideFlags.HideAndDontSave; // 에디터에서만 표시하고 저장 안 함
            //SetPreviewMaterial(_previewObject); // 반투명하게 보이도록 설정
        }
    }

    private void StopPainting()
    {
        _isPainting = false;

        SceneView.duringSceneGui -= OnSceneGUI;

        if (_previewObject != null)
        {
            DestroyImmediate(_previewObject);
            _previewObject = null;
        }
    }
    private void OnSceneGUI(SceneView sceneView)
    {
        if (!_isPainting || _selectedPrefab == null) return;

        Event e = Event.current;
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 targetPosition = hit.point;

            if (_previewObject != null)
            {
                _previewObject.transform.position = targetPosition;
            }

            if (e.type == EventType.MouseDown && e.button == 0)
            {
                PlaceObject(targetPosition);
                e.Use(); 
            }
        }

        SceneView.RepaintAll();
    }

    private void PlaceObject(Vector3 position)
    {
        if (_selectedPrefab == null) return;

        GameObject root = GameObject.Find("ObjectRoot");
        if (root == null) root = new GameObject("ObjectRoot");

        GameObject newObject = Instantiate(_selectedPrefab, position, Quaternion.identity, root.transform);
        Undo.RegisterCreatedObjectUndo(newObject, "Placed Object");
    }

    private void SavePrefabList()
    {
        List<string> prefabPaths = new List<string>();
        foreach (var prefab in _prefabList)
        {
            if (prefab != null)
            {
                prefabPaths.Add(AssetDatabase.GetAssetPath(prefab));
            }
        }

        EditorPrefs.SetString("ObjectPrefabList", string.Join(";", prefabPaths));
    }

    private void LoadPrefabList()
    {
        _prefabList.Clear();

        if (EditorPrefs.HasKey("ObjectPrefabList"))
        {
            string savedData = EditorPrefs.GetString("ObjectPrefabList");
            string[] prefabPaths = savedData.Split(';');

            foreach (string path in prefabPaths)
            {
                if (!string.IsNullOrEmpty(path))
                {
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (prefab != null)
                    {
                        _prefabList.Add(prefab);
                    }
                }
            }
        }
    }


    #region 실행 취소 관리
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
    #endregion

    #region 에디팅 후 총 변경사항 저장 및 환경 정리

    private void SavePrefab()
    {
        // 여기 걸리면 안된다.
        if (targetInstance == null)
        {
            Debug.LogError("StageMap이 설정되지 않았습니다. 프리팹 저장 불가.");
            return;
        }

        ClearAllIndicators();

        if (PrefabUtility.IsPartOfPrefabInstance(targetInstance))
        {
            PrefabUtility.ApplyPrefabInstance(targetInstance.gameObject, InteractionMode.UserAction);
            Debug.Log("StageMap 프리팹 인스턴스의 변경 사항이 원본 프리팹에 저장되었습니다.");
        }
        else
        {
            Debug.LogWarning("님꺼 연결 안됐음. 씬 오브젝트일 가능성이 높습니다.");
        }
    }

    private void TryCloseEditor()
    {
        if (!HasUnsavedChanges())
        {
            ClearEnvAndClose();
            return;
        }

        int choice = EditorUtility.DisplayDialogComplex(
            "변경 사항 감지됨",
            "변경된 내용이 있습니다. 저장하고 닫으시겠습니까?",
            "예 (저장 후 닫기)",
            "아니오 (저장 없이 닫기)",
            "취소"
        );

        switch (choice)
        {
            case 0: 
                SaveSpawnPoints();

                SavePrefab();
                ClearEnvAndClose();
                break;

            case 1:
                ClearEnvAndClose();
                break;

            case 2:
            default:
                break;
        }
    }
    private bool HasUnsavedChanges()
    {
        if (targetInstance == null || targetStageMap == null) return false;

        SerializedObject serializedTarget = new(targetInstance);
        SerializedProperty currentData = serializedTarget.FindProperty("battleSpawnerData");

        SerializedObject originalTarget = new(targetStageMap);
        SerializedProperty originalData = originalTarget.FindProperty("battleSpawnerData");

        return !SerializedProperty.DataEquals(currentData, originalData);
    }

    public void ClearEnvAndClose()
    {
        ClearAllIndicators();
        if (targetInstance != null)
        {
            Undo.ClearUndo(targetInstance);
            DestroyImmediate(targetInstance.gameObject);
            targetInstance = null;
        }

        Close();
    }
    #endregion
}
*/

/// <summary>
/// StageEditor는 Stage(SpawnPoint) 에디팅과 오브젝트 배치를 위한 EditorWindow입니다.
/// 내부에서는 SpawnPointEditor와 ObjectPlacer로 GUI와 기능을 위임합니다.
/// </summary>
public class StageEditor : EditorWindow
{
    public enum ePlacementMode
    {
        SpawnPoint,
        Object
    }

    private const string TARGET_SCENE_PATH = "Assets/Scenes/Editing/LevelEditingScene.unity";
    private const string PREF_SPAWN_INDICATOR = "PREF_SPAWN_INDICATOR";

    /// <summary> 현재 편집할 맵의 프리팹이다. </summary>
    /// <remarks> <b>저장 및 로드 용도로만 사용함에 주의</b> </remarks>
    private StageField targetStageMap;

    /// <summary> 현재 올라가 있는 프리팹의 인스턴스이다. </summary>
    /// <remarks> <b>이거로만 내부에서 편집 작업한다!</b> </remarks>
    private StageField targetInstance;

    /// <summary> 배치 모드. 무엇을 배치할 건가요? </summary>
    private ePlacementMode currentPlacementMode = ePlacementMode.SpawnPoint;

    /// <summary> 에디터 내 기능별 모듈 1 : SpawnPoint를 지정 </summary>
    private SpawnPointEditor spawnPointEditor;

    /// <summary> 에디터 내 기능별 모듈 2 : Object를 배치 </summary>
    [SerializeField]
    private StageObjectEditor objectPlacer;

    #region SpawnPointEditor 초기화용
    private SpawnIndicator spawnIndicatorPrefab;
    #endregion

    #region ObjectPlacer 제어용
    //[SerializeField] private List<GameObject> prefabList = new List<GameObject>();
    //private SerializedProperty prefabListProperty;
    //private SerializedObject serializedObjectRef;
    #endregion

    private Vector2 scrollPosition;

    [MenuItem("Tools/Stage Editor")]
    public static void ShowWindow()
    {
        StageEditor window = GetWindow<StageEditor>();
        window.titleContent = new GUIContent("Stage Editor");
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            EditorSceneManager.OpenScene(TARGET_SCENE_PATH);
        else
            window.Close();
    }

    private void OnEnable()
    {
        //serializedObjectRef = new SerializedObject(this);
        //prefabListProperty = serializedObjectRef.FindProperty("prefabList");

        #region Module Initialization
        LoadSpawnIndicatorPrefab();
        spawnPointEditor = new SpawnPointEditor(spawnIndicatorPrefab);
        spawnPointEditor.LoadColorPreferences();

        objectPlacer = new StageObjectEditor();
        objectPlacer.LoadPrefabList();
        #endregion

        #region Basic Subscribe
        EditorSceneManager.sceneOpened += OnSceneOpened;        
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
        Undo.undoRedoPerformed += OnUndoRedo;
        #endregion

        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SaveSpawnIndicatorPrefab();
        spawnPointEditor.SaveColorPreferences();
        objectPlacer.SavePrefabList();

        EditorSceneManager.sceneOpened -= OnSceneOpened;
        SceneView.duringSceneGui -= OnSceneGUI;
        EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        Undo.undoRedoPerformed -= OnUndoRedo;
    }

    private void OnGUI()
    {
        // 모드 선택 토글
        currentPlacementMode = (ePlacementMode)GUILayout.Toolbar((int)currentPlacementMode,
            new string[] { "Spawn Point Mode", "Object Mode" });

        EditorGUILayout.HelpBox("맵 에디팅 완료 후 저장 시 반드시 Save Map을 누르고 닫아주세요.", MessageType.Warning);
        GUILayout.Space(10);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        // StageMap 선택 및 로드
        EditorGUILayout.LabelField("레벨 디자인 맵 선택", EditorStyles.boldLabel);
        StageField newStageMap = EditorGUILayout.ObjectField("Target StageMap", targetStageMap, typeof(StageField), true) as StageField;
        if (newStageMap != targetStageMap)
        {
            targetStageMap = newStageMap;
            LoadStageMapIntoScene();
        }

        GUILayout.Space(10);

        // 현재 배치 모드에 따른 GUI 위임
        switch (currentPlacementMode)
        {
            case ePlacementMode.SpawnPoint:
                spawnPointEditor.DrawGUI();
                break;
            case ePlacementMode.Object:
                objectPlacer.DrawGUI();
                break;
        }

        EditorGUILayout.EndScrollView();
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Save Map"))
        {
            SaveMap();
        }
        if (GUILayout.Button("Close Editor"))
        {
            TryCloseEditor();
        }
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (targetInstance == null) return;
        switch (currentPlacementMode)
        {
            case ePlacementMode.SpawnPoint:
                spawnPointEditor.OnSceneGUI(sceneView, targetInstance);
                break;
            case ePlacementMode.Object:
                objectPlacer.OnSceneGUI(sceneView);
                break;
        }
    }

    private void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        if (scene.path != TARGET_SCENE_PATH)
        {
            EditorApplication.delayCall += () => {
                EditorSceneManager.OpenScene(TARGET_SCENE_PATH);
                Debug.LogError("목표 씬 외 이동은 허용되지 않습니다.");
            };
        }
    }

    private void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            if (SceneManager.GetActiveScene().path == TARGET_SCENE_PATH)
            {
                EditorApplication.isPlaying = false;
                Debug.LogError("LevelEditingScene에서는 게임 실행이 불가합니다.");
            }
        }
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            Debug.Log("게임 실행으로 Stage Editor 창이 닫힙니다.");
            Close();
        }
    }

    private void LoadStageMapIntoScene()
    {
        if (targetStageMap == null) return;
        if (SceneManager.GetActiveScene().path != TARGET_SCENE_PATH)
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(TARGET_SCENE_PATH);
            else
            {
                Debug.LogWarning("씬 전환 취소: 현재 씬 저장 안됨.");
                return;
            }
        }
        if (targetInstance != null)
        {
            DestroyImmediate(targetInstance.gameObject);
        }
        GameObject targetGO = PrefabUtility.InstantiatePrefab(targetStageMap.gameObject) as GameObject;
        targetInstance = targetGO.GetComponent<StageField>();
        targetInstance.gameObject.name = "StageMap_Editor_Instance";
        SceneManager.MoveGameObjectToScene(targetInstance.gameObject, SceneManager.GetActiveScene());
        Undo.RegisterCreatedObjectUndo(targetInstance.gameObject, "Load StageMap for Editing");
        Debug.Log("StageMap이 로드되었습니다.");
        spawnPointEditor.RestoreSpawnIndicators(targetInstance);
    }

    private void SaveSpawnIndicatorPrefab()
    {
        if (spawnIndicatorPrefab != null)
        {
            string path = AssetDatabase.GetAssetPath(spawnIndicatorPrefab);
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
            spawnIndicatorPrefab = AssetDatabase.LoadAssetAtPath<SpawnIndicator>(path);
        }
    }

    private void SaveMap()
    {
        if (targetInstance == null)
        {
            Debug.LogError("StageMap이 설정되지 않았습니다. 저장 불가.");
            return;
        }

        spawnPointEditor.SaveSpawnPoints(targetInstance);

        // 저장 전에 Spawn Point 데이터 업데이트 (필요에 따라 spawnPointEditor에서 저장)
        spawnPointEditor.ClearAllIndicators();
        if (PrefabUtility.IsPartOfPrefabInstance(targetInstance))
        {
            PrefabUtility.ApplyPrefabInstance(targetInstance.gameObject, InteractionMode.UserAction);
            Debug.Log("StageMap 프리팹 인스턴스 변경사항이 저장되었습니다.");
        }
        else
        {
            Debug.LogWarning("씬 오브젝트일 가능성이 높습니다.");
        }
    }

    private void TryCloseEditor()
    {
        if (!HasUnsavedChanges())
        {
            ClearEnvironmentAndClose();
            return;
        }
        int choice = EditorUtility.DisplayDialogComplex("변경 사항 감지됨", "변경된 내용이 있습니다. 저장 후 닫으시겠습니까?", "예 (저장 후 닫기)", "아니오 (저장 없이 닫기)", "취소");
        switch (choice)
        {
            case 0:
                spawnPointEditor.SaveSpawnPoints(targetInstance);
                SaveMap();
                ClearEnvironmentAndClose();
                break;
            case 1:
                ClearEnvironmentAndClose();
                break;
            default:
                break;
        }
    }

    private bool HasUnsavedChanges()
    {
        if (targetInstance == null || targetStageMap == null) return false;
        SerializedObject currentSO = new SerializedObject(targetInstance);
        SerializedProperty currentData = currentSO.FindProperty("battleSpawnerData");
        SerializedObject originalSO = new SerializedObject(targetStageMap);
        SerializedProperty originalData = originalSO.FindProperty("battleSpawnerData");
        return !SerializedProperty.DataEquals(currentData, originalData);
    }

    private void ClearEnvironmentAndClose()
    {
        spawnPointEditor.ClearAllIndicators();
        if (targetInstance != null)
        {
            Undo.ClearUndo(targetInstance);
            DestroyImmediate(targetInstance.gameObject);
            targetInstance = null;
        }
        Close();
    }

    private void OnUndoRedo()
    {
        spawnPointEditor.CleanupIndicators();
        Repaint();
    }
}




#endif
