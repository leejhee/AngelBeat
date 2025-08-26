using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace novel
{
    class NovelAudioProvider : SettingsProvider
    {
        private SerializedObject novelSettings;
        private string path = NovelEditorUtils.GetNovelResourceDataPath(NovelDataType.Audio);
        public NovelAudioProvider(string path, SettingsScope scope = SettingsScope.Project) : base(path, scope) { }

        private bool showBGM = true;
        private bool showSFX = true;

        public override void OnActivate(string searchContext, UnityEngine.UIElements.VisualElement rootElement)
        {
            novelSettings = NovelEditorUtils.GetSerializedSettings<NovelAudioData>(path);
        }

        public override void OnGUI(string searchContext)
        {
            if (novelSettings == null)
                novelSettings = NovelEditorUtils.GetSerializedSettings<NovelAudioData>(path);

            novelSettings.Update();

            EditorGUILayout.LabelField("Audio Resources", EditorStyles.boldLabel);

            //showBGM = EditorGUILayout.Foldout(showBGM, "BGM List");

            EditorGUILayout.LabelField("BGM List");
            var bgmProp = novelSettings.FindProperty("bgmDict").FindPropertyRelative("pairs");

            if (showBGM)
            {
                EditorGUI.indentLevel++;


                for (int i = 0 ; i < bgmProp.arraySize; i++)
                {
                    var element = bgmProp.GetArrayElementAtIndex(i);
                    var keyProp = element.FindPropertyRelative("key");
                    var valueProp = element.FindPropertyRelative("value");

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(keyProp, GUIContent.none);
                    EditorGUILayout.PropertyField(valueProp, GUIContent.none);

                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("+", GUILayout.Width(18)))
            {
                bgmProp.arraySize++;
            }
            if (GUILayout.Button("-", GUILayout.Width(18)))
            {
                bgmProp.DeleteArrayElementAtIndex(bgmProp.arraySize - 1);
            }
            EditorGUILayout.EndHorizontal();



            // 여기부터 SFX

            EditorGUILayout.LabelField("SFX List");
            var sfxProp = novelSettings.FindProperty("sfxDict").FindPropertyRelative("pairs");

            if (showSFX)
            {
                EditorGUI.indentLevel++;


                for (int i = 0; i < sfxProp.arraySize; i++)
                {
                    var element = sfxProp.GetArrayElementAtIndex(i);
                    var keyProp = element.FindPropertyRelative("key");
                    var valueProp = element.FindPropertyRelative("value");

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(keyProp, GUIContent.none);
                    EditorGUILayout.PropertyField(valueProp, GUIContent.none);

                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("+", GUILayout.Width(18)))
            {
                sfxProp.arraySize++;
            }
            if (GUILayout.Button("-", GUILayout.Width(18)))
            {
                sfxProp.DeleteArrayElementAtIndex(sfxProp.arraySize - 1);
            }
            EditorGUILayout.EndHorizontal();

            novelSettings.ApplyModifiedPropertiesWithoutUndo();
        }

        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            return new NovelAudioProvider("Project/Novel/Audio", SettingsScope.Project)
            {
                keywords = new System.Collections.Generic.HashSet<string>(new[] { "Audio" })
            };
        }
    }
}
