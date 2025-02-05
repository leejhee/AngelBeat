using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

[Serializable]
public partial class CharStatData : SheetData
{
    public long index; // 캐릭터 스탯 ID
	public int strength; // 근력
	public int agility; // 민첩
	public int intel; // 지능
	

    public override Dictionary<long, SheetData> LoadData()
    {
        var dataList = new Dictionary<long, SheetData>();

        string ListStr = null;
		int line = 0;
        TextAsset csvFile = Resources.Load<TextAsset>($"CSV/MEMCSV/{this.GetType().Name}");
        try
		{            
            string csvContent = csvFile.text;
            string[] lines = Regex.Split(csvContent, @"(?=[^""]*,""[^""]*"",)\n");

            for (int i = 3; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                    continue;

                string[] values = Regex.Split(lines[i].Trim(),
                                        @",(?=(?:[^""\[\]]*(?:""[^""]*""|[\[][^\]]*[\]])?)*[^""\[\]]*$)");
                for (int j = 0; j < values.Length; j++)
                    {
                        values[j] = Regex.Replace(values[j], @"^""|""$", "");
                    }

                line = i;

                CharStatData data = new CharStatData();

                
				if(values[0] == "")
				    data.index = default;
				else
				    data.index = Convert.ToInt64(values[0]);
				
				if(values[1] == "")
				    data.strength = default;
				else
				    data.strength = Convert.ToInt32(values[1]);
				
				if(values[2] == "")
				    data.agility = default;
				else
				    data.agility = Convert.ToInt32(values[2]);
				
				if(values[3] == "")
				    data.intel = default;
				else
				    data.intel = Convert.ToInt32(values[3]);
				

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