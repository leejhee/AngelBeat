using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

public partial class CharData : SheetData
{
    public long index; // 캐릭터 ID
	public string charName; // 캐릭터 이름(추후 stringcode로 교체할 것)
	public string charPrefabName; // 캐릭터 프리팹 루트
	public string charImage; // 캐릭터 아이콘 루트
	public List<long> charSkillList; // 캐릭터 스킬 ID 리스트
	public long charStat; // 캐릭터 스탯 ID
	public eCharType defaultCharType; // 캐릭터 타입
	

    public override Dictionary<long, SheetData> LoadData()
    {
        var dataList = new Dictionary<long, SheetData>();

        string ListStr = null;
		int line = 0;
        TextAsset csvFile = Resources.Load<TextAsset>($"CSV/MEMCSV/{this.GetType().Name}");
        try
		{            
            string csvContent = csvFile.text;
            string[] lines = Regex.Split(csvContent, @"(?<!""[^""]*)\r?\n");

            for (int i = 3; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                    continue;

                string[] values = Regex.Split(lines[i].Trim(),
                                        @",(?=(?:[^""\[\]]*(?:""[^""]*""|[\[][^\]]*[\]])?)*[^""\[\]]*$)")
                                        .Select(column => column.Trim())
                                        .Select(column => Regex.Replace(column, @"^""|""$", ""))
                                        .ToArray();
                line = i;

                CharData data = new CharData();

                
				if(values[0] == "")
				    data.index = default;
				else
				    data.index = Convert.ToInt64(values[0]);
				
				if(values[2] == "")
				    data.charName = default;
				else
				    data.charName = Convert.ToString(values[2]);
				
				if(values[3] == "")
				    data.charPrefabName = default;
				else
				    data.charPrefabName = Convert.ToString(values[3]);
				
				if(values[4] == "")
				    data.charImage = default;
				else
				    data.charImage = Convert.ToString(values[4]);
				
				ListStr = values[5].Replace('[',' ');
				ListStr = ListStr.Replace(']', ' ');
				var charSkillListData = ListStr.ToString().Split(',').Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)).Select(x => Convert.ToInt64(x)).ToList();
				data.charSkillList = charSkillListData;
				
				if(values[6] == "")
				    data.charStat = default;
				else
				    data.charStat = Convert.ToInt64(values[6]);
				
				if(values[7] == "")
				    data.defaultCharType = default;
				else
				    data.defaultCharType = (eCharType)Enum.Parse(typeof(eCharType), values[7]);
				

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