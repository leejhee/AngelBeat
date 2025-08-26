#if UNITY_EDITOR

using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine.AddressableAssets;
public enum NovelDataType
{
    Audio,
    Background,
    Character,
    Variable,
    Script
}
public static class NovelEditorUtils
{
    public const string NovelRoot = "Assets/Addressables/Novel";
    public const string NovelResourcePath = NovelRoot + "/NovelResourceData";


    public static T GetOrCreateSettings<T> (string path) where T : ScriptableObject
    {
        var settings = AssetDatabase.LoadAssetAtPath<T>(path);
        if (settings == null)
        {
            settings = ScriptableObject.CreateInstance<T>();


            AssetDatabase.CreateAsset(settings, path);
            AssetDatabase.SaveAssets();

            // 어드레서블로 설정 일단 해줄필요는 없는데 나중에 필요할시 수정할것
            //AddToAddressables(path, address ?? Path.GetFileNameWithoutExtension(path));
        }
        return settings;
    }
    public static SerializedObject GetSerializedSettings<T>(string path) where T : ScriptableObject
    {
        return new SerializedObject(GetOrCreateSettings<T>(path));
    }

    public static T LoadSettingsOrNull<T>(string path) where T : ScriptableObject
    {
        return AssetDatabase.LoadAssetAtPath<T>(path);
    }

    public static void EnsureFolders(string assetPath)
    {
        var dir = Path.GetDirectoryName(assetPath)?.Replace("\\", "/");
        if (string.IsNullOrEmpty(dir)) return;
        if (AssetDatabase.IsValidFolder(dir)) return;

        var parts = dir.Split('/');
        string current = parts[0]; // "Assets"
        for (int i = 1; i < parts.Length; i++)
        {
            string next = $"{current}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }

    public static string GetNovelDataPath(NovelDataType type)
    {
        string path = NovelResourcePath + "/Novel" + type.ToString() + "Data.asset";
        return path;
    }

    //private static void AddToAddressables(string assetPath, string address)
    //{
    //    var settings = AddressableAssetSettingsDefaultObject.Settings;
    //    if (settings == null)
    //    {
    //        Debug.LogError("AddressableAssetSettings not found. Make sure Addressables is set up.");
    //        return;
    //    }

    //    var group = settings.DefaultGroup;
    //    var guid = AssetDatabase.AssetPathToGUID(assetPath);
    //    var entry = settings.CreateOrMoveEntry(guid, group);
    //    entry.address = address;

    //    EditorUtility.SetDirty(settings);
    //    AssetDatabase.SaveAssets();

    //}
}
#endif