using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Data;
using System.Linq;
using System.IO;

[Serializable]
public class BaseMapNodeDataList : ScriptableObject, ITableSO
{
    [SerializeField]
    private List<BaseMapNodeData> objects = new(); // 필드로 정의
    public List<BaseMapNodeData> Objects => objects; // 타입별 리스트

    public void DataInitialize()
    {
        objects.Clear();
        TextAsset csvFile = Resources.Load<TextAsset>($"CSV/SOCSV/{typeof(BaseMapNodeData)}");
        int line = 0;
        string ListStr = null;
        try
        {
            string csv = csvFile.text;
            string[] rows = csv.Split('\n');
            for (int i = 3; i < rows.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(rows[i]))
                    continue;

                string[] values = rows[i].Trim().Split(',');
                line = i;

                BaseMapNodeData data = new();
                
                
				if(values[0] == "")
				    data.index = default;
				else
				    data.index = Convert.ToInt32(values[0]);
				
				if(values[1] == "")
				    data.nodeType = default;
				else
				    data.nodeType = (SystemEnum.eNodeType)Enum.Parse(typeof(SystemEnum.eNodeType), values[1]);
				
				if(values[2] == "")
				    data.nodeSprite = null;
				else
				    data.nodeSprite = Resources.Load<Sprite>($"Sprites/BaseMapNodeData/{values[2]}");
				
				if(values[3] == "")
				    data.selectable = default;
				else
				    data.selectable = Convert.ToBoolean(values[3]);
				

                Objects.Add(data);
            }
        }
        catch (Exception)
        {
            Debug.LogError($"{GetType().Name}의 {line}전후로 데이터 문제 발생");
        }
    }
}