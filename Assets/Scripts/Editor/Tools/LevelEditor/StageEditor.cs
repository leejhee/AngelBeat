#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AngelBeat.Tools.LevelEditor
{
    /// <summary>
    /// StageEditor는 Stage(SpawnPoint) 에디팅과 오브젝트 배치를 위한 EditorWindow입니다.
    /// 내부에서는 SpawnPointEditor와 ObjectPlacer로 GUI와 기능을 위임합니다.
    /// </summary>
    public class StageEditor : EditorWindow
    {
        private enum ePlacementMode
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
                    objectPlacer.OnSceneGUI(sceneView, targetInstance);
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
            if (!targetStageMap) return;
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
            if (targetInstance)
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
            if (!targetInstance)
            {
                Debug.LogError("StageMap이 설정되지 않았습니다. 저장 불가.");
                return;
            }

            spawnPointEditor.SaveSpawnPoints(targetInstance);
            objectPlacer.SaveObjects(targetInstance);

            // 저장 전에 Spawn Point 데이터 업데이트 (필요에 따라 spawnPointEditor에서 저장)
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
            if (!targetInstance || !targetStageMap) return false;
            SerializedObject currentSO = new(targetInstance);
            SerializedProperty currentData = currentSO.FindProperty("battleSpawnerData");
            SerializedObject originalSO = new(targetStageMap);
            SerializedProperty originalData = originalSO.FindProperty("battleSpawnerData");
            return !SerializedProperty.DataEquals(currentData, originalData);
        }

        private void ClearEnvironmentAndClose()
        {
            spawnPointEditor.ClearAllIndicators();
            if (targetInstance)
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

}



#endif
