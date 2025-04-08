#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// StageObjectEditor는 오브젝트 배치 모드에서 프리팹 선택, 미리보기 및 배치를 위한 GUI와 기능을 담당합니다.
/// </summary>
[System.Serializable]
public class StageObjectEditor
{
    [SerializeField] private List<GameObject> prefabList = new List<GameObject>();
    public List<GameObject> PrefabList => prefabList;
    public GameObject SelectedPrefab { get; private set; }
    public bool IsPainting { get; private set; } = false;
    private GameObject previewObject;

    [SerializeField] private List<FieldObjectInfo> placedObjects = new List<FieldObjectInfo>();

    public void DrawGUI()
    {
        EditorGUILayout.LabelField("오브젝트 배치 모드", EditorStyles.boldLabel);

        // prefabList를 직접 표시 및 수정
        for (int i = 0; i < prefabList.Count; i++)
        {
            prefabList[i] = (GameObject)EditorGUILayout.ObjectField("Prefab " + i, prefabList[i], typeof(GameObject), false);
            if (prefabList[i] is not null)
            {
                if (GUILayout.Button(prefabList[i].name))
                    SelectedPrefab = prefabList[i];
                if (SelectedPrefab == prefabList[i])
                {
                    GUILayout.Space(5);
                    Texture2D previewTexture = AssetPreview.GetAssetPreview(SelectedPrefab);
                    if (previewTexture is not null)
                        GUILayout.Label(previewTexture, GUILayout.Width(100), GUILayout.Height(100));
                }
            }
        }
        if (GUILayout.Button("Add Prefab"))
            prefabList.Add(null);
        if (GUILayout.Button("Clear List"))
            prefabList.Clear();

        if (SelectedPrefab is not null)
            EditorGUILayout.LabelField("현재 선택된 프리팹: " + SelectedPrefab.name);
        else
            EditorGUILayout.LabelField("현재 선택된 프리팹이 없습니다.");

        GUILayout.Space(10);

        if (!IsPainting && SelectedPrefab is not null)
        {
            if (GUILayout.Button("Start Painting", GUILayout.Height(30)))
                StartPainting();
        }
        else if (IsPainting)
        {
            if (GUILayout.Button("Stop Painting", GUILayout.Height(30)))
                StopPainting();
        }
    }

    public void OnSceneGUI(SceneView sceneView, StageField targetInstance)
    {
        if (!IsPainting || SelectedPrefab == null) return;

        Event e = Event.current;
        Rect sceneRect = new(0, 0, sceneView.position.width, sceneView.position.height);
        bool mouseInScene = sceneRect.Contains(e.mousePosition);

        if (previewObject != null)
            previewObject.SetActive(mouseInScene);

        if (!mouseInScene)
        {
            sceneView.Repaint();
            return;
        }

        Vector3 worldPos = HandleUtility.GUIPointToWorldRay(e.mousePosition).origin;
        worldPos.z = 0; // z=0 평면에 배치한다고 가정

        if (previewObject != null)
            previewObject.transform.position = worldPos;

        if (e.type == EventType.MouseDown && e.button == 0)
        {
            PlaceObject(worldPos, targetInstance.ObjectRoot);
            e.Use();
        }

        sceneView.Repaint();
    }

    public void StartPainting()
    {
        if (!SelectedPrefab) return;
        IsPainting = true;
        if (!previewObject)
        {
            previewObject = UnityEngine.Object.Instantiate(SelectedPrefab);
            previewObject.name = "PreviewObject";
        }
    }

    public void StopPainting()
    {
        IsPainting = false;
        if (previewObject != null)
        {
            UnityEngine.Object.DestroyImmediate(previewObject);
            previewObject = null;
        }
    }

    /// <summary>
    /// PlaceObject는 선택된 프리팹을 주어진 위치에 인스턴스화하고, 배치 정보를 기록합니다.
    /// </summary>
    public void PlaceObject(Vector3 position, Transform parent=null)
    {
        if (SelectedPrefab == null) return;
        GameObject newObj = UnityEngine.Object.Instantiate(SelectedPrefab, position, Quaternion.identity, parent);
        Undo.RegisterCreatedObjectUndo(newObj, "Place Object");

        FieldObjectInfo info = new (SelectedPrefab.name, newObj.transform.position);
        placedObjects.Add(info);
    }

    public void SaveObjects(StageField targetInstance)
    {
        if (targetInstance == null)
        {
            Debug.LogError("StageMap이 설정되지 않았습니다. Object 저장 불가.");
            return;
        }

        Undo.RecordObject(targetInstance, "Save Object Placements");

        SerializedObject so = new(targetInstance);
        SerializedProperty spawnerProp = so.FindProperty("battleSpawnerData");
        if (spawnerProp == null)
        {
            Debug.LogError("battleSpawnerData를 찾을 수 없습니다.");
            return;
        }

        BattleFieldSpawnInfo info = spawnerProp.managedReferenceValue as BattleFieldSpawnInfo;
        if (info == null)
            info = new BattleFieldSpawnInfo();

        info.fieldObjectInfos.Clear();
        foreach (FieldObjectInfo objInfo in placedObjects)
        {
            info.fieldObjectInfos.Add(objInfo);
        }

        spawnerProp.managedReferenceValue = info;
        so.ApplyModifiedProperties();

        Debug.Log("Object 배치 정보가 StageMap에 저장되었습니다.");
        EditorUtility.SetDirty(targetInstance);
    }

    public void SavePrefabList()
    {
        List<string> paths = new List<string>();
        foreach (GameObject prefab in prefabList)
        {
            if (prefab != null)
                paths.Add(AssetDatabase.GetAssetPath(prefab));
        }
        EditorPrefs.SetString("ObjectPrefabList", string.Join(";", paths));
    }

    public void LoadPrefabList()
    {
        prefabList.Clear();
        if (EditorPrefs.HasKey("ObjectPrefabList"))
        {
            string saved = EditorPrefs.GetString("ObjectPrefabList");
            string[] pathArray = saved.Split(';');
            foreach (string path in pathArray)
            {
                if (!string.IsNullOrEmpty(path))
                {
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (prefab != null)
                        prefabList.Add(prefab);
                }
            }
        }
    }
}
#endif
