using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class NovelValueSetting : ScriptableObject
{
    public const string novelValueSettingsPath = "Assets/Novel/Editor/NovelValueSetting.asset";

    [SerializeField]
    private SerializableDict<string, float> novelValuesDict;
    [SerializeField]
    private int testNum;

    internal static NovelValueSetting GetOrCreateSettings()
    {
        var settings = AssetDatabase.LoadAssetAtPath<NovelValueSetting>(novelValueSettingsPath);
        if (settings ==  null)
        {
            settings = ScriptableObject.CreateInstance<NovelValueSetting>();
            settings.novelValuesDict = new();
            settings.testNum = 1;
            AssetDatabase.CreateAsset(settings, novelValueSettingsPath);
            AssetDatabase.SaveAssets();
        }
        return settings;
    }
    internal static SerializedObject GetSerializedSettings()
    {
        return new SerializedObject(GetOrCreateSettings());
    }
}

static class MyCustomSettingsUIElementsRegister
{
    [SettingsProvider]
    public static SettingsProvider CreateMyCustomSettingsProvider()
    {
        var provider = new SettingsProvider("Project/NovelSettings", SettingsScope.Project)
        {
            // By default the last token of the path is used as display name if no label is provided.
            label = "Novel",
            // Create the SettingsProvider and initialize its drawing (IMGUI) function in place:
            guiHandler = (searchContext) =>
            {
                var settings = NovelValueSetting.GetSerializedSettings();
                EditorGUILayout.PropertyField(settings.FindProperty("testNum"), new GUIContent("My Number"));
                settings.ApplyModifiedPropertiesWithoutUndo();
            },

            // Populate the search keywords to enable smart search filtering and label highlighting:
            keywords = new HashSet<string>(new[] { "Number" })
        };

        return provider;
    }
}
class NovelSettingsProvider : SettingsProvider
{
    private SerializedObject novelSettings;

    class Styles
    {
        public static GUIContent number = new GUIContent("My Number");
        public static GUIContent someString = new GUIContent("Some string");
    }
    public const string novelValueSettingsPath = "Assets/Novel/Editor/NovelValueSetting.asset";

    public NovelSettingsProvider(string path, SettingsScope scope = SettingsScope.User)
    : base(path, scope) { }
    public static bool IsSettingsAvailable()
    {
        return File.Exists(novelValueSettingsPath);
    }
    public override void OnActivate(string searchContext, VisualElement rootElement)
    {
        // This function is called when the user clicks on the MyCustom element in the Settings window.
        novelSettings = NovelValueSetting.GetSerializedSettings();
    }
    public override void OnGUI(string searchContext)
    {
        // Use IMGUI to display UI:
        EditorGUILayout.PropertyField(novelSettings.FindProperty("testNum"), Styles.number);;
        novelSettings.ApplyModifiedPropertiesWithoutUndo();
    }

    [SettingsProvider]
    public static SettingsProvider CreateMyCustomSettingsProvider()
    {
        if (IsSettingsAvailable())
        {
            var provider = new NovelSettingsProvider("Project/NovelSettingsProvider", SettingsScope.Project);

            // Automatically extract all keywords from the Styles.
            provider.keywords = GetSearchKeywordsFromGUIContentProperties<Styles>();
            return provider;
        }

        // Settings Asset doesn't exist yet; no need to display anything in the Settings window.
        return null;
    }
}