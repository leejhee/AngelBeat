using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using novel;
public class NovelScriptProvider : SettingsProvider
{
    SerializedObject novelScript;
    private string path = NovelEditorUtils.GetNovelDataPath(NovelDataType.Script);

    public NovelScriptProvider(string path, SettingsScope scope = SettingsScope.Project) : base(path, scope) { }
    public override void OnActivate(string searchContext, UnityEngine.UIElements.VisualElement rootElement)
    {
        novelScript = NovelEditorUtils.GetSerializedSettings<NovelScriptData>(path);
    }

    public override void OnGUI(string searchContext)
    {
        if (novelScript == null)
            novelScript = NovelEditorUtils.GetSerializedSettings<NovelScriptData>(path);

        novelScript.Update();

        EditorGUILayout.PropertyField(novelScript.FindProperty("scriptList"), new GUIContent("Script List"));

        novelScript.ApplyModifiedPropertiesWithoutUndo();
    }

    [SettingsProvider]
    public static SettingsProvider CreateProvider()
    {
        return new NovelScriptProvider("Project/Novel/Script", SettingsScope.Project)
        {
            keywords = new System.Collections.Generic.HashSet<string>(new[] { "Script" })
        };
    }
}
