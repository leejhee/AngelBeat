using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine.Networking;
using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Creating ScriptableObject of MapNodeData from Sheet 
/// </summary>
public class SheetToSO : EditorWindow
{
    [MenuItem("Tools/Import Scriptable Object Data")]
    public static void ShowWindow()
    {
        GetWindow<SheetToSO>("Import SO data from Google Sheets");
    }

    private const string ADDRESS = "https://docs.google.com/spreadsheets/d/";
    private string spreadsheetId;
    private string gid;

    private Dictionary<string, string> typeBringer;
    private int idx = 0;
    private string[] dataTypes;

    private bool initialized = false;

    private void OnEnable()
    {
        #region Init Dictionary of SO Type with GID Pair
        typeBringer = new Dictionary<string, string>();
        string filePath = Application.dataPath + "/Resources/ScriptableObjects/ImportDescription.txt";
        if (File.Exists(filePath)) 
        {
            string description = File.ReadAllText(filePath);
            var pair = description.Split(' ');
            spreadsheetId = pair[0]; gid = pair[1];
            EditorCoroutineUtility.StartCoroutine(ParseSheet(true), this);
        }
        #endregion
    }


    /// <summary>
    /// GUI 설정하는 부분
    /// </summary>
    private void OnGUI()
    {
        if(initialized)
        {
            EditorGUILayout.BeginVertical();
            #region Get Input
            GUILayout.Label("Google Sheets Importer(Only for Scriptable Objects!)", EditorStyles.boldLabel);
            idx = EditorGUILayout.Popup("Select Type", idx, dataTypes);
            #endregion
            #region Buttons below
            if (GUILayout.Button("Import Data for SO"))
            {               
                gid = typeBringer[dataTypes[idx]];
                EditorCoroutineUtility.StartCoroutine(ParseSheet(), this);
            }

            if (GUILayout.Button("Create Empty SO to Asset"))
            {
                CreateEmptySO(dataTypes[idx]);
            }

            #endregion
            EditorGUILayout.EndVertical();
        }       
    }

    private IEnumerator ParseSheet(bool init = false)
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
                Debug.Log("Data retrieved: \n" + importedCSV);
                if (!init)
                {                   
                    string selectedType = dataTypes[idx];
                    CSVToClass(importedCSV, selectedType);
                }
                else // 가져올 타입 목록 초기화
                {
                    InitSheetAddresses(importedCSV);
                }
            }
        }
    }

    /// <summary> 드롭다운 리스트업 위한 시트 id 초기화 </summary>
    private void InitSheetAddresses(string idTable)
    {
        var rows = idTable.Split("\r\n");
        foreach (var row in rows)
        {
            var elements = row.Split(',');
            typeBringer.Add(elements[0], elements[1]);
        }
        dataTypes = typeBringer.Keys.ToArray();
        initialized = true;
    }

    /// <summary> CSV 저장 및 SO 클래스용 스크립트 작성. 타입 이름은 시트에서 가져옴 </summary>
    private void CSVToClass(string csv, string dataType)
    {
        // CSV 테이블화
        var rows = csv.Split("\r\n");
        var fieldComments = rows[0].Split(",");
        var fieldNames =    rows[1].Split(",");
        var fieldTypes =    rows[2].Split(",");

        #region Write Script
        StringBuilder fieldDeclaration = new();
        StringBuilder fieldParsing = new();

        for (int col = 0; col < fieldNames.Length; col++)
        {
            var comment = fieldComments[col];
            var name = fieldNames[col];
            var type = fieldTypes[col];

            if (comment[0] == '#' || name[0] == '#' || type[0] == '#') continue;

            var toMemberType = ToMemberType(type.Replace("[]", ""));
            if (toMemberType != string.Empty)
            {
                // 리스트 타입 처리
                if (type.EndsWith("[]"))
                {
                    string elementType = type.Replace("[]", "");
                    fieldDeclaration.Append(string.Format(DataClassFormat.dataRegisterListFormat, elementType, name, comment) + Environment.NewLine);
                    fieldParsing.Append(string.Format(DataClassFormat.dataParseListFormat, col.ToString(), name, ToMemberType(elementType)) + Environment.NewLine);
                }
                else
                {
                    // 기본 자료형
                    fieldDeclaration.Append(string.Format(DataClassFormat.dataRegisterFormat, type, name, comment) + Environment.NewLine);
                    fieldParsing.Append(string.Format(DataClassFormat.dataParseFomat, col.ToString(), name, toMemberType) + Environment.NewLine);
                }
            }
            else
            {
                // Enum 전용
                fieldDeclaration.Append(string.Format(DataClassFormat.dataEnumRegisterFormat, type, name, comment) + Environment.NewLine);
                fieldParsing.Append(string.Format(DataClassFormat.dataEnumParseFomat, col.ToString(), name, type) + Environment.NewLine);
            }            
        }

        fieldDeclaration = fieldDeclaration.Replace("\n", "\n\t");
        fieldParsing = fieldParsing.Replace("\n", "\n\t\t\t\t");
        try
        {
            var dataScript = string.Format(DataClassFormat.SODataFormat, dataType, fieldDeclaration.ToString());
            File.WriteAllText($"{Application.dataPath}/Scripts/ScriptableObj/{dataType}.cs", dataScript);
            var dataContainerScript = string.Format(DataClassFormat.SOContainerFormat, dataType, fieldDeclaration.ToString(), fieldParsing.ToString());
            File.WriteAllText($"{Application.dataPath}/Scripts/ScriptableObj/{dataType}List.cs", dataContainerScript);

            Debug.Log($"코드 작성 완료. {dataType}.cs");
            File.WriteAllText($"{Application.dataPath}/Resources/CSV/SOCSV/{dataType}.csv", csv);
            Debug.Log($"CSV 저장 완료. {dataType}.csv");
        }
        catch(FormatException e)
        {
            Debug.Log(e.Message);
        }

        
        #endregion      
    }

    /// <summary> 원활한 타입 변환 위한 string 변환기 </summary>
    private static string ToMemberType(string memberType)
    {
        switch (memberType)
        {
            case "bool":
            case "Bool":
                return "ToBoolean";
            // case "byte": // 따로 정의되어있지는 않음
            case "short":
            case "Short":
                return "ToInt16";
            case "ushort":
            case "Ushort":
                return "ToUInt16";
            case "int":
            case "Int":
                return "ToInt32";
            case "uint":
            case "Uint":
                return "ToUInt32";
            case "long":
            case "Long":
                return "ToInt64";
            case "ulong":
            case "Ulong":
                return "ToUInt64";
            case "float":
            case "Float":
                return "ToSingle";
            case "double":
            case "Double":
                return "ToDouble";
            case "string":
            case "String":
                return "ToString";
            default:
                return string.Empty;
        }
    }

    private void CreateEmptySO(string dataType)
    {
        Type type = Type.GetType($"{dataType}, Assembly-CSharp");
        Type containerType = Type.GetType($"{dataType}List, Assembly-CSharp");
        if (type == null || containerType == null)
        {
            Debug.LogError("먼저 들여오고 누르셨나요??");
            return;
        }
      
        var objcontainer = CreateInstance(containerType.Name);
        AssetDatabase.CreateAsset(objcontainer, $"Assets/Resources/ScriptableObjects/{dataType}List.asset");
        EditorUtility.SetDirty(objcontainer);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}


