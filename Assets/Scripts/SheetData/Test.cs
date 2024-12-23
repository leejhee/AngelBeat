using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Data;
using System.Linq;


public partial class Test : SheetData
{
    public int index; // testindex
	public string first; // testvalue1
	public float second; // testvalue2
	

    public override Dictionary<long, SheetData> LoadData()
    {
        var dataList = new Dictionary<long, SheetData>();

        string ListStr = null;
		int line = 0;
        TextAsset csvFile = Resources.Load<TextAsset>($"CSV/{this.GetType().Name}");
        try
		{            
            string csvContent = csvFile.text;
            string[] lines = csvContent.Split('\n');
            for (int i = 3; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                    continue;

                string[] values = lines[i].Trim().Split(',');
                line = i;

                Test data = new Test();

                
				if(values[0] == "")
				    data.index = default;
				else
				    data.index = Convert.ToInt32(values[0]);
				
				if(values[1] == "")
				    data.first = default;
				else
				    data.first = Convert.ToString(values[1]);
				
				if(values[2] == "")
				    data.second = default;
				else
				    data.second = Convert.ToSingle(values[2]);
				

                dataList[data.index] = data;
            }

            return dataList;
        }
		catch (Exception e)
		{
			Debug.LogError($"{this.GetType().Name}의 {line}전후로 데이터 문제 발생");
			return new Dictionary<long, SheetData>();
		}
    }
}