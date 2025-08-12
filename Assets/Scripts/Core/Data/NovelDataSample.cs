using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Core.Data
{
    [Serializable]
    public partial class NovelDataSample : Data.SheetData
    {
public long index; // 인덱스
	public string command; // 커맨드
	public string script; // 스크립트 내용
	

        public override Dictionary<long, Data.SheetData> LoadData()
        {
            var dataList = new Dictionary<long, Data.SheetData>();

            string ListStr = null;
			int line = 0;
            TextAsset csvFile = Resources.Load<TextAsset>($"CSV/MEMTSV/{this.GetType().Name}");
            try
			{            
                string csvContent = csvFile.text;
                var lines = Regex.Split(csvContent, @"(?<!""[^""]*)\r?\n");
                for (int i = 3; i < lines.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(lines[i]))
                        continue;

                    string[] values = lines[i].Trim().Split('\t');
                    line = i;

                    NovelDataSample data = new NovelDataSample();

                    
				if(values[0] == "")
				    data.index = default;
				else
				    data.index = Convert.ToInt64(values[0]);
				
				if(values[1] == "")
				    data.command = default;
				else
				    data.command = Convert.ToString(values[1]);
				
				if(values[2] == "")
				    data.script = default;
				else
				    data.script = Convert.ToString(values[2]);
				

                    dataList[data.index] = data;
                }

                return dataList;
            }
			catch (Exception e)
			{
				Debug.LogError($"{this.GetType().Name}의 {line}전후로 데이터 문제 발생");
				return new Dictionary<long, Data.SheetData>();
			}
        }
    }
}