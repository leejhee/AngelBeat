using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class Util
{
    public static T GetOrAddComponent<T>(GameObject go) where T : UnityEngine.Component
    {
        T component = go.GetComponent<T>();
        if (component == null)
        {
            component = go.AddComponent<T>();
        }
        return component;
    }
    public static T FindChild<T>(GameObject go, string name = null, bool recursive = false) where T : UnityEngine.Object
    {
        if (go == null)
            return null;

        if (recursive == false)
        {
            for (int i = 0; i < go.transform.childCount; i++)
            {
                Transform transform = go.transform.GetChild(i);
                if (string.IsNullOrEmpty(name) || transform.name == name)
                {
                    T component = transform.GetComponent<T>();
                    if (component != null)
                        return component;
                }
            }
        }
        else
        {
            foreach (T component in go.GetComponentsInChildren<T>())
            {
                if (string.IsNullOrEmpty(name) || component.name == name)
                    return component;
            }
        }

        return null;
    }
    public static GameObject FindChild(GameObject go, string name = null, bool recursive = false)
    {
        Transform transform = FindChild<Transform>(go, name, recursive);
        if (transform == null)
            return null;

        return transform.gameObject;
    }

    public static void SaveJson<DataClass>(DataClass dataClass, string fileName = null) where DataClass : class
    {
        string jsonText = null;

        if (string.IsNullOrEmpty(fileName))
        {
            fileName = typeof(DataClass).Name;

            int index = fileName.IndexOf("DataClass");
            if(index != -1)
            {
                fileName = string.Concat(fileName.Substring(0, index), 's');
            }
        }

        string savePath;
        string appender = "/userdata";
        string nameString = $"/{fileName}.json";

#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE
        savePath = Application.dataPath;
#elif UNITY_ANDROID
        savePath = Application.persistentDataPath;
#endif

        StringBuilder stringBuilder = new StringBuilder(savePath);
        stringBuilder.Append(appender);
        if (!Directory.Exists(stringBuilder.ToString()))
        {
            Debug.Log("No such directory");
            Directory.CreateDirectory(stringBuilder.ToString());
        }
        stringBuilder.Append(nameString);

        jsonText = JsonUtility.ToJson(dataClass, true);
        Debug.Log(jsonText);

        using(FileStream fileStream = new FileStream(stringBuilder.ToString(), FileMode.Create))
        {
            byte[] bytes = Encoding.UTF8.GetBytes(jsonText);
            fileStream.Write(bytes, 0, bytes.Length);
            fileStream.Close();
        }
    }

    public static DataClass LoadSaveData<DataClass>(string fileName = null) where DataClass : class
    {
        if (string.IsNullOrEmpty(fileName))
        {
            fileName = typeof(DataClass).Name;

            int index = fileName.IndexOf("DataClass");
            if (index != -1)
            {
                fileName = string.Concat(fileName.Substring(0, index), 's');
            }
        }

        DataClass gameData;
        string loadPath;
        string appender = "/userdata";
        string nameString = $"/{fileName}.json";
#if UNITY_ANDROID
        loadPath = Application.persistentDataPath;
#endif        
        loadPath = Application.dataPath;

        StringBuilder stringBuilder = new StringBuilder(loadPath);
        stringBuilder.Append(appender);
        if (!Directory.Exists(stringBuilder.ToString()))
        {
            Debug.Log("No such directory. Returns nothing");
            Directory.CreateDirectory(stringBuilder.ToString());
            return default(DataClass);
        }
        stringBuilder.Append(nameString);

        if (File.Exists(stringBuilder.ToString()))
        {
            using(FileStream filestream = new FileStream(stringBuilder.ToString(), FileMode.Open))
            {
                byte[] bytes = new byte[filestream.Length];
                filestream.Read(bytes, 0, bytes.Length);
                filestream.Close();
                string jsonData = Encoding.UTF8.GetString(bytes);
                gameData = JsonUtility.FromJson<DataClass>(jsonData);
            }
        }
        else
        {
            gameData = default(DataClass);
        }
        return gameData;
    }
}
