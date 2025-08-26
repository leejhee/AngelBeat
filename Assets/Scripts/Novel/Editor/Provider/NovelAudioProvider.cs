using Core.Foundation.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace novel
{
    class NovelAudioProvider : SettingsProvider
    {
        private SerializedObject novelSettings;
        private string path = NovelEditorUtils.GetNovelDataPath(NovelDataType.Audio);
        public NovelAudioProvider(string path, SettingsScope scope = SettingsScope.Project) : base(path, scope) { }

        public override void OnActivate(string searchContext, UnityEngine.UIElements.VisualElement rootElement)
        {
            novelSettings = NovelEditorUtils.GetSerializedSettings<NovelAudioData>(path);
        }

        public override void OnGUI(string searchContext)
        {
            if (novelSettings == null)
                novelSettings = NovelEditorUtils.GetSerializedSettings<NovelAudioData>(path);

            novelSettings.Update();

            EditorGUILayout.PropertyField(novelSettings.FindProperty("bgmDict"), new GUIContent("BGM List"));
            EditorGUILayout.PropertyField(novelSettings.FindProperty("sfxDict"), new GUIContent("SFX List"));


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
