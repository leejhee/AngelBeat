using Codice.Client.BaseCommands;
using Core.Foundation.Utils;
using UnityEditor;
using UnityEngine;

namespace novel
{
    class NovelVariableProvider : SettingsProvider
    {
        private SerializedObject novelSettings;
        private string path = NovelEditorUtils.GetNovelResourceDataPath(NovelDataType.Variable);
        public NovelVariableProvider(string path, SettingsScope scope = SettingsScope.Project) : base(path, scope) { }

        public override void OnActivate(string searchContext, UnityEngine.UIElements.VisualElement rootElement)
        {
            novelSettings = NovelEditorUtils.GetSerializedSettings<NovelVariableData>(path);
        }

        public override void OnGUI(string searchContext)
        {
            if (novelSettings == null)
                novelSettings = NovelEditorUtils.GetSerializedSettings<NovelVariableData>(path);

            novelSettings.Update();

            EditorGUILayout.PropertyField(novelSettings.FindProperty("novelVariableDict"), new GUIContent("Variable List"));

            novelSettings.ApplyModifiedPropertiesWithoutUndo();
        }

        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            return new NovelVariableProvider("Project/Novel/Variable", SettingsScope.Project)
            {
                keywords = new System.Collections.Generic.HashSet<string>(new[] { "Number" })
            };
        }
    }

}
