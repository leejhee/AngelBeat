#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace Client
{
    public class CharGenerator : EditorWindow
    {
        Object SPUM_Object = null;

        Dictionary<long, SheetData> CharDict;       
        string[] Options;
        int selectedOptionIndex;

        string targetPath = "Assets/Resources/Prefabs/Char/";

        Editor SPUMEditor;

        [MenuItem("Tools/CharGenerator")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(CharGenerator), false, "Character Generator");
        }

        void OnEnable()
        {
            DataManager.Instance.DataLoad();
            CharDict = DataManager.Instance.GetDictionary("CharData");
            List<string> options = new();
            foreach(var value in CharDict.Values)
            {
                CharData data = value as CharData;
                options.Add($"charName : {data.charName} - index : {data.index}");
            }
            Options = options.ToArray();
        }

        void OnDisable()
        {
            AssetDatabase.SaveAssets();
            DataManager.Instance.ClearCache();
        }

        void OnGUI()
        {
            GUILayout.Label("캐릭터 생성 툴", EditorStyles.boldLabel);

            #region Put SPUM Object
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("SPUM 원형 캐릭터를 넣어주세요.");
            GUILayout.FlexibleSpace();
            SPUM_Object = EditorGUILayout.ObjectField(SPUM_Object, typeof(GameObject), false, GUILayout.MaxWidth(300));
            EditorGUILayout.EndHorizontal();
            #endregion

            EditorGUILayout.HelpBox("SPUM 원형 캐릭터에 캐릭터 필수 기능을 제작하여 넣습니다.", MessageType.Info);

            #region Select Data for New Prefab
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("어떤 캐릭터의 프리팹인가요?");
            GUILayout.FlexibleSpace();
            selectedOptionIndex = EditorGUILayout.Popup(selectedOptionIndex, Options, GUILayout.MaxWidth(300));
            EditorGUILayout.EndHorizontal();
            #endregion

            #region Select Path
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("프리팹 이름을 제외하고 입력해주세요.");
            GUILayout.FlexibleSpace();
            EditorGUILayout.TextField(targetPath, GUILayout.MaxWidth(500));
            EditorGUILayout.EndHorizontal();
            #endregion

            #region Set Preview of Prefab
            if (SPUM_Object != null)
            {
                if (SPUMEditor == null || SPUMEditor.target != SPUM_Object)
                {
                    DestroyImmediate(SPUMEditor); // 주석 처리된 부분 그대로 유지
                    SPUMEditor = Editor.CreateEditor(SPUM_Object);
                }
                SPUMEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(100, 100), GUIStyle.none);
            }
            #endregion


            GUILayout.FlexibleSpace();
            if (GUILayout.Button("캐릭터 생성 또는 기능 추가", GUILayout.Width(300)))
            {
                GeneratorChar();
            }   
        }

        /// <summary>
        /// 이후 기능 추가가 필요할 수 있음 반드시 이를 생각하고 작업할 것
        /// </summary>
        private void GeneratorChar()
        {
            long selectedId = new List<long>(CharDict.Keys)[selectedOptionIndex];
            CharData targetData = CharDict[selectedId] as CharData;

            if (SPUM_Object is null)
            {
                Debug.LogError("먼저 선택하고 누르셨나요?");
                return;
            }

            #region COPY SELECTED SPUM ASSET
            string assetPath = AssetDatabase.GetAssetPath(SPUM_Object);
            string savePath = targetPath + targetData.charPrefabName + ".prefab";
            if (string.IsNullOrEmpty(assetPath))
            {
                Debug.LogError("에셋이 아니래요!");               
                return;
            }
            else if (File.Exists(savePath))
            {
                Debug.LogError($"중복되는 이름의 파일이 프리팹 폴더에 존재합니다. {targetPath}를 확인해주세요.");
                return;
            }
            #endregion

            #region LOAD COPIED ASSET AND EDIT

            GameObject CharPrefab = new GameObject(targetData.charPrefabName);
            CharPrefab.transform.position = new Vector3(0, 1f, 0);

            #region TEMPORARY OBJECT - SPUMPrefab
            GameObject SPUMPrefab = PrefabUtility.InstantiatePrefab(SPUM_Object) as GameObject;
            if (!SPUMPrefab)
            {
                Debug.LogError("복사하고 로드했는데 null? 뭔가 잘못됨. 여기에 걸리면 안됨.");
                return;
            }
            PrefabUtility.UnpackPrefabInstance(SPUMPrefab, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

            List<Transform> children = new List<Transform>();
            foreach(Transform t in SPUMPrefab.transform.GetComponentInChildren<Transform>())
                children.Add(t);           
            foreach (Transform child in children)
            {
                child.SetParent(CharPrefab.transform);
                child.position = new Vector3(0, 1f, 0);
            }

            DestroyImmediate(SPUMPrefab);
            #endregion

            CharBase newCharBase = CharFactory.AddBaseComponent(targetData, CharPrefab);
            SerializedObject serialized = new SerializedObject(newCharBase);

            SerializedProperty Index = serialized.FindProperty("_index");
            Index.longValue = targetData.index;
            serialized.ApplyModifiedProperties();

            CharFactory.CharacterizeBase(CharPrefab);
            CharPrefab.name = targetData.charPrefabName;

            PrefabUtility.SaveAsPrefabAsset(CharPrefab, savePath);
            #endregion
            Debug.Log($"성공적으로 {CharPrefab.name} 저장했습니다");
            DestroyImmediate(CharPrefab);            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

        }

    }

    /// <summary> 에디터에서 CharBase 상속하는 캐릭터 뽑는 용 </summary>
    public static class CharFactory
    {
        public static CharBase AddBaseComponent(CharData data, GameObject go)
        {
            var capsule = go.AddComponent<CapsuleCollider2D>();
            capsule.offset = new Vector2(0, 0.5f);
            capsule.size = new Vector2(0.5f, 1f);
            go.AddComponent<Rigidbody2D>();

            switch (data.defaultCharType)
            {
                case SystemEnum.eCharType.Neutral:
                case SystemEnum.eCharType.Player:
                    return go.AddComponent<CharPlayer>();
                case SystemEnum.eCharType.Enemy:
                    return go.AddComponent<CharMonster>();
                default:
                    return null;
            }
        }

        // 캐릭터 내 필요한 하위 오브젝트 및 컴포넌트 조정
        public static void CharacterizeBase(GameObject go)
        {
            CharBase charGo = go.GetComponent<CharBase>();
            SerializedObject obj = new SerializedObject(charGo);

            //////////UnitRoot(Animator)/////////
            Animator anim = go.GetComponentInChildren<Animator>();
            SerializedProperty animator = obj.FindProperty("_Animator");
            animator.objectReferenceValue = anim;

            //////////FightCollider//////////////
            GameObject Descendant = new GameObject("BattleCollider");
            Descendant.transform.SetParent(go.transform, false);
            CapsuleCollider col = Descendant.AddComponent<CapsuleCollider>();
            col.radius = 0.25f;
            col.center = new Vector3(0, 0.5f, 0);

            SerializedProperty FightCollider = obj.FindProperty("_battleCollider");
            FightCollider.objectReferenceValue = col;

            //////////SkillRoot//////////////
            Descendant = new GameObject("SkillRoot");
            Descendant.transform.SetParent(go.transform, false);

            SerializedProperty SkillRoot = obj.FindProperty("_SkillRoot");
            SkillRoot.objectReferenceValue = Descendant;

            //////////CameraPos//////////////
            Descendant = new GameObject("CameraPos");
            Descendant.transform.SetParent(go.transform, false);

            SerializedProperty CamaraPos = obj.FindProperty("_CharCameraPos");
            CamaraPos.objectReferenceValue = Descendant;

            obj.ApplyModifiedProperties();

        }
    }
}
#endif