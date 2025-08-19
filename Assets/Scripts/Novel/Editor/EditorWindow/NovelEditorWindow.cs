using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class NovelEditorWindow
{
    [MenuItem("Novel/Variable")]
    public static void OpenVariableMenu()
    {
        SettingsService.OpenProjectSettings("Project/Novel/Variable");
    }

    [MenuItem("Novel/Audio")]
    public static void OpenAudioMenu()
    {
        SettingsService.OpenProjectSettings("Project/Novel/Audio");
    }

    [MenuItem("Novel/Background")]
    public static void OpenBackgroundMenu()
    {
        SettingsService.OpenProjectSettings("Project/Novel/Background");
    }
}
