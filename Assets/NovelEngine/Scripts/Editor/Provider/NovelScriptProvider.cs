using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using novel;
public class NovelScriptProvider : SettingsProvider
{
    SerializedObject novelScript;
    private string path = NovelEditorUtils.GetNovelResourceDataPath(NovelDataType.Script);

    public NovelScriptProvider(string path, SettingsScope scope = SettingsScope.Project) : base(path, scope) { }

    private bool showScript = true;
    public override void OnActivate(string searchContext, UnityEngine.UIElements.VisualElement rootElement)
    {
        novelScript = NovelEditorUtils.GetSerializedSettings<NovelScriptData>(path);
    }

    public override void OnGUI(string searchContext)
    {
        if (novelScript == null)
            novelScript = NovelEditorUtils.GetSerializedSettings<NovelScriptData>(path);

        novelScript.Update();

        EditorGUILayout.LabelField("Script List");
        var scriptProp = novelScript.FindProperty("scriptList").FindPropertyRelative("pairs");

        if (showScript)
        {
            EditorGUI.indentLevel++;


            for (int i = 0; i < scriptProp.arraySize; i++)
            {
                var element = scriptProp.GetArrayElementAtIndex(i);
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
            scriptProp.arraySize++;
        }
        if (GUILayout.Button("-", GUILayout.Width(18)))
        {
            scriptProp.DeleteArrayElementAtIndex(scriptProp.arraySize - 1);
        }
        EditorGUILayout.EndHorizontal();

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
