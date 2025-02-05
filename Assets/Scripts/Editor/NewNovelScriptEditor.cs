//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEditor;
//using System.IO;

//namespace novel
//{
//    public class NewNovelScriptEditor : EditorWindow
//    {

//        private string scriptTitle = "New Script";
//        private List<DialogueLine> dialogueLines = new();
//        private List<NovelCommand> commands = new();

//        private Vector2 scrollPos = Vector2.zero;


//        [MenuItem("Novel/New Script")]
//        private static void NovelScripts()
//        {
//            GetWindow<NewNovelScriptEditor>("New Script");
//        }
//        private void OnGUI()
//        {
//            GUILayout.Label("Novel Script Editor", EditorStyles.boldLabel);
//            GUILayout.Space(5);

//            scriptTitle = EditorGUILayout.TextField("Script Title", scriptTitle);

//            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);




//            for (int i = 0; i < dialogueLines.Count; i++)
//            {
//                GUILayout.Space(10);
//                EditorGUILayout.BeginVertical("box");

//                EditorGUILayout.BeginHorizontal();
//                //인덱스
//                GUILayout.Label($"{i + 1}", GUILayout.Width(25));
//                dialogueLines[i].index = i + 1;

//                // 커맨드 드롭다운

//                dialogueLines[i].command = (CommandType)EditorGUILayout.EnumPopup(dialogueLines[i].command, GUILayout.Width(100));

//                // 스크립트 작성 공간
//                dialogueLines[i].dialogue = EditorGUILayout.TextArea(dialogueLines[i].dialogue, GUILayout.Height(40));
//                EditorGUILayout.EndHorizontal();


//                if (GUILayout.Button("Remove"))
//                {
//                    dialogueLines.RemoveAt(i);
//                }
//                EditorGUILayout.EndVertical();
//            }

//            if (GUILayout.Button("New Dialog Line"))
//            {
//                dialogueLines.Add(new DialogueLine());
//            }


//            EditorGUILayout.EndScrollView();

//            GUILayout.Space(20);

//            if (GUILayout.Button("Save Script"))
//            {
//                SaveScript();
//            }
//        }

//        private void SaveScript()
//        {
//            if (dialogueLines.Count == 0)
//            {
//                EditorUtility.DisplayDialog("Error", "스크립트가 비어있음", "OK");
//                return;
//            }

//            // 파일 저장 경로 설정
//            string path = "Assets/NovelScriptData";
//            if (!AssetDatabase.IsValidFolder(path))
//            {
//                AssetDatabase.CreateFolder("Assets", "NovelScriptData");
//            }

//            string fileName = $"{scriptTitle}.asset";
//            string fullPath = $"{path}/{fileName}";


//            // 동명의 파일이 존재하는 경우 덮어쓰기
//            NovelScript existingScript = AssetDatabase.LoadAssetAtPath<NovelScript>(fullPath);

//            if (existingScript != null)
//            {
//                bool overwrite = EditorUtility.DisplayDialog("파일이 이미 존재합니다",
//                    $"{scriptTitle} 파일이 이미 존재합니다. 덮어쓰겠습니까?", "예", "취소");

//                if (!overwrite)
//                {
//                    Debug.Log("저장 취소");
//                    return;
//                }
//                else
//                {
//                    Debug.Log("덮어쓰기");
//                }

//                EditorUtility.CopySerialized(ScriptableObject.CreateInstance<NovelScript>(), existingScript);
//                existingScript.scriptTitle = scriptTitle;
//                existingScript.dialogueLines = dialogueLines;
//                EditorUtility.SetDirty(existingScript);
//            }
//            else
//            {
//                NovelScript newScript = ScriptableObject.CreateInstance<NovelScript>();
//                newScript.scriptTitle = scriptTitle;
//                newScript.dialogueLines = dialogueLines;

//                AssetDatabase.CreateAsset(newScript, fullPath);
//            }

//            // 🔹 저장 후 갱신
//            AssetDatabase.SaveAssets();
//            AssetDatabase.Refresh();

//            Debug.Log($"{fullPath} 저장 완료");
//        }
//    }


//}
