using Core.Foundation.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace novel
{
    public class NovelBackgroundProvider : SettingsProvider
    {
        private SerializedObject novelSettings;

        private string path = NovelEditorUtils.GetNovelDataPath(NovelDataType.Background);
        public NovelBackgroundProvider(string path, SettingsScope scope = SettingsScope.Project) : base(path, scope) { }

        public override void OnActivate(string searchContext, UnityEngine.UIElements.VisualElement rootElement)
        {
            novelSettings = NovelEditorUtils.GetSerializedSettings<NovelBackgroundData>(path);
        }

        public override void OnGUI(string searchContext)
        {
            if (novelSettings == null)
                novelSettings = NovelEditorUtils.GetSerializedSettings<NovelBackgroundData>(path);

            novelSettings.Update();

            EditorGUILayout.PropertyField(novelSettings.FindProperty("novelBackgroundDict"), new GUIContent("Background List"));

            novelSettings.ApplyModifiedPropertiesWithoutUndo();
        }

        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            return new NovelBackgroundProvider("Project/Novel/Background", SettingsScope.Project)
            {
                keywords = new System.Collections.Generic.HashSet<string>(new[] { "Number" })
            };
        }
    }

}
