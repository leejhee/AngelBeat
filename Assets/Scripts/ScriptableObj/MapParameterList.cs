using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Data;
using System.Linq;
using System.IO;

[Serializable]
public class MapParameterList : ScriptableObject, ITableSO
{
    [SerializeField]
    private List<MapParameter> objects = new(); // 필드로 정의
    public List<MapParameter> Objects => objects; // 타입별 리스트

    public void DataInitialize()
    {
        objects.Clear();
        TextAsset csvFile = Resources.Load<TextAsset>($"CSV/SOCSV/{typeof(MapParameter)}");
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

                MapParameter data = new();
                
                
				if(values[0] == "")
				    data.index = default;
				else
				    data.index = Convert.ToInt32(values[0]);
				
				if(values[1] == "")
				    data.mapName = default;
				else
				    data.mapName = Convert.ToString(values[1]);
				
				if(values[2] == "")
				    data.maxDepth = default;
				else
				    data.maxDepth = Convert.ToInt32(values[2]);
				
				if(values[3] == "")
				    data.trialNum = default;
				else
				    data.trialNum = Convert.ToInt32(values[3]);
				
				if(values[4] == "")
				    data.width = default;
				else
				    data.width = Convert.ToInt32(values[4]);
				
				ListStr = values[5].Replace('[',' ');
				ListStr = ListStr.Replace(']', ' ');
				var availablePointsData = ListStr.ToString().Split('.').Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)).Select(x => Convert.ToInt32(x)).ToList();
				data.availablePoints = availablePointsData;
				
				ListStr = values[6].Replace('[',' ');
				ListStr = ListStr.Replace(']', ' ');
				var avaliableEventsData = ListStr.ToString().Split('.').Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)).Select(x => Convert.ToInt32(x)).ToList();
				data.avaliableEvents = avaliableEventsData;
				

                Objects.Add(data);
            }
        }
        catch (Exception)
        {
            Debug.LogError($"{GetType().Name}의 {line}전후로 데이터 문제 발생");
        }
    }
}