using Core.Foundation.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace novel
{
    public class NovelCharacterProvider : SettingsProvider
    {
        SerializedObject novelChar;
        private string path = NovelEditorUtils.GetNovelDataPath(NovelDataType.Character);
        public NovelCharacterProvider(string path, SettingsScope scope = SettingsScope.Project) : base(path, scope) { }

        public override void OnActivate(string searchContext, UnityEngine.UIElements.VisualElement rootElement)
        {
            novelChar = NovelEditorUtils.GetSerializedSettings<NovelCharacterData>(path);
        }

        public override void OnGUI(string searchContext)
        {
            if (novelChar == null)
                novelChar = NovelEditorUtils.GetSerializedSettings<NovelCharacterData>(path);


            novelChar.Update();

            SerializedProperty dictProp = novelChar.FindProperty("charDict");
            EditorGUILayout.LabelField("Character List", EditorStyles.boldLabel);
            for (int i = 0; i < dictProp.arraySize; i++)
            {
                SerializedProperty element = dictProp.GetArrayElementAtIndex(i);
                SerializedProperty keyProp = element.FindPropertyRelative("key");
                SerializedProperty valueProp = element.FindPropertyRelative("value");

                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.BeginHorizontal();

                // 캐릭터 이름 표시
                EditorGUILayout.PropertyField(keyProp, GUIContent.none);

                // 토글 버튼
                bool showDetail = EditorPrefs.GetBool($"NovelCharDetail_{keyProp.stringValue}", false);
                if (GUILayout.Button(showDetail ? "▲" : "▼", GUILayout.Width(25)))
                {
                    showDetail = !showDetail;
                    EditorPrefs.SetBool($"NovelCharDetail_{keyProp.stringValue}", showDetail);
                }

                EditorGUILayout.EndHorizontal();

                // 펼친 상태일 때만 상세 정보 표시
                if (showDetail)
                {
                    EditorGUILayout.PropertyField(valueProp, new GUIContent("Character Data"), true);
                }

                EditorGUILayout.EndVertical();
            }

            novelChar.ApplyModifiedPropertiesWithoutUndo();
        }

        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            return new NovelCharacterProvider("Project/Novel/Character", SettingsScope.Project)
            {
                keywords = new System.Collections.Generic.HashSet<string>(new[] { "Character" })
            };
        }
    }

}
