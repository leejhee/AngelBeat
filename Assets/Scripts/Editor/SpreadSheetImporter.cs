using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine.Networking;
using UnityEngine;
using System.Collections;

#pragma warning disable IDE0051
/// <summary>
/// Creating ScriptableObject of MapNodeData from Sheet 
/// </summary>
public class SheetToSO : EditorWindow
{
    [MenuItem("Tools/Import Map Data")]
    public static void ShowWindow()
    {
        GetWindow<SheetToSO>("Google Sheets Importer");
    }

    [Header("주소 변경 시 수정 필요")]
    public string gidDescription = 
        "SheetID : 1KM9AWtBiVDo0_dQswSNqZs9zOJ7Vt6AS5LfHifA9d5s\n" +
        "MapNodeData : 257691777\n";

    private string ADDRESS = "https://docs.google.com/spreadsheets/d/";
    private string spreadsheetId = "YOUR_SPREADSHEET_ID";
    private string gid = "YOUR_API_KEY";

    private void OnGUI()
    {
        GUILayout.Label("Google Sheets Importer", EditorStyles.boldLabel);
        spreadsheetId = EditorGUILayout.TextField("AddressID", spreadsheetId);
        gid = EditorGUILayout.TextField("GID", gid);
        //이름을 적어야하나..? 싶은데 고민할것.

        EditorGUILayout.TextArea(gidDescription, GUILayout.Height(100));

        if (GUILayout.Button("Import Data"))
        {
            EditorCoroutineUtility.StartCoroutine(SheetToCSV(), this);
        }

    }

    // gid에 따라서 찾아낸 시트를 CSV로 가져온다. 
    // [TODO] : 시트 gid마다 구분해서 올바른 SO를 만들 수 있도록 
    private IEnumerator SheetToCSV()
    {
        string url = ADDRESS + spreadsheetId + $"/export?format=csv&gid={gid}";

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + www.error);
            }
            else
            {
                string importedCSV = www.downloadHandler.text;
                Debug.Log("Data retrieved: " + importedCSV);
            }
        }
    }
}
#pragma warning restore IDE0051
