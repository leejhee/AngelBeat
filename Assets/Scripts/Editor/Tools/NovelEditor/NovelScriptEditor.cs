using UnityEditor;
using UnityEngine;
using UnityEditor.Callbacks;
using System.Collections.Generic;

namespace novel
{
    public class NovelScriptEditor : EditorWindow
    {
        private static NovelScriptEditor instance;
        private NovelScript novelScript;
        private NovelPlayer novelPlayer;
        private Vector2 scrollPos;

        [MenuItem("Novel/Edit Script")]
        private static void ShowWindow()
        {
            instance = GetWindow<NovelScriptEditor>("Edit Script");
            EditorApplication.update += instance.RepaintEditor;
        }

        private void OnEnable()
        {
            EditorApplication.update += RepaintEditor;
        }

        private void OnDisable()
        {
            EditorApplication.update -= RepaintEditor;
        }

        [OnOpenAsset(1)]
        public static bool OpenNovelScript(int instanceID, int line)
        {
            Object obj = EditorUtility.InstanceIDToObject(instanceID);

            if (obj is NovelScript script)
            {
                if (instance == null)
                {
                    ShowWindow();
                }
                instance.novelScript = script;
                instance.Repaint();
                return true;
            }
            return false;
        }

        private void OnGUI()
        {
            GUILayout.Label("Novel Script Editor", EditorStyles.boldLabel);
            GUILayout.Space(5);
            GUILayout.Label("수정하는 즉시 저장됩니다.");

            novelScript = (NovelScript)EditorGUILayout.ObjectField("Script", novelScript, typeof(NovelScript), false);

            if (novelScript == null)
            {
                EditorGUILayout.HelpBox("대화 스크립트 SO를 선택해주세요.", MessageType.Warning);
                return;
            }

            if (novelScript.dialogueLines == null)
            {
                novelScript.dialogueLines = new List<DialogueLine>();
            }

            if (novelPlayer == null)
            {
                novelPlayer = FindObjectOfType<NovelPlayer>();
            }

            int currentLineIndex = novelPlayer != null ? novelPlayer.GetCurrentLineIndex() : -1;

            string newScriptTitle = EditorGUILayout.TextField("Script Title", novelScript.scriptTitle);
            if (newScriptTitle != novelScript.scriptTitle && !string.IsNullOrWhiteSpace(newScriptTitle))
            {
                novelScript.scriptTitle = newScriptTitle;
                AutoSave();
            }

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            for (int i = 0; i < novelScript.dialogueLines.Count; i++)
            {
                if (i == currentLineIndex)
                {
                    GUI.backgroundColor = Color.yellow;
                }
                else
                {
                    GUI.backgroundColor = Color.white;
                }

                GUILayout.Space(10);
                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label($"{i + 1}", GUILayout.Width(25));
                novelScript.dialogueLines[i].index = i + 1;

                CommandType newCommand = (CommandType)EditorGUILayout.EnumPopup(novelScript.dialogueLines[i].command, GUILayout.Width(100));
                if (newCommand != novelScript.dialogueLines[i].command)
                {
                    novelScript.dialogueLines[i].command = newCommand;
                    AutoSave();
                }

                string newDialogue = EditorGUILayout.TextArea(novelScript.dialogueLines[i].dialogue, GUILayout.Height(40));
                if (newDialogue != novelScript.dialogueLines[i].dialogue)
                {
                    novelScript.dialogueLines[i].dialogue = newDialogue;
                    AutoSave();
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Remove", GUILayout.Width(80)))
                {
                    novelScript.dialogueLines.RemoveAt(i);
                    AutoSave();

                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndScrollView();
                    return;
                }

                if (GUILayout.Button("Add Below", GUILayout.Width(100)))
                {
                    DialogueLine newLine = new DialogueLine
                    {
                        index = novelScript.dialogueLines[i].index + 1,
                        dialogue = novelScript.dialogueLines[i].dialogue, // 기존 대사 복사
                        command = novelScript.dialogueLines[i].command // 기존 명령어 복사
                    };

                    novelScript.dialogueLines.Insert(i + 1, newLine);
                    AutoSave();

                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndScrollView();
                    return;
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                GUI.backgroundColor = Color.white;
            }

            if (GUILayout.Button("Add Dialog Line"))
            {
                novelScript.dialogueLines.Add(new DialogueLine());
                AutoSave();
            }

            EditorGUILayout.EndScrollView();
        }

        private void RepaintEditor()
        {
            if (instance != null)
            {
                instance.Repaint();
            }
        }

        private void AutoSave()
        {
            if (novelScript == null) return;

            Undo.RecordObject(novelScript, "Auto Save");
            EditorUtility.SetDirty(novelScript);
            AssetDatabase.SaveAssets();
        }
    }
}
