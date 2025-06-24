using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

[Serializable]
public partial class ExecutionData : SheetData
{
    public long index; // 효과 ID
	public SystemEnum.eExecutionType executionType; // 효과 분류
	public SystemEnum.eStats targetStat; // 효과 적용 대상 스탯
	public SystemEnum.eExecutionPhase executionPhase; // 실행 시점
	public int executionDuration; // 지속되는 턴 수
	public int prob; // 적용 확률
	public long input1; // 1번입력
	public long input2; // 2번입력
	public long input3; // 3번입력
	public long input4; // 4번입력
	public long nextCondition; // 연쇄 실행 조건
	public List<long> nextExecutionList; // 연쇄 실행 Execution List
	

    public override Dictionary<long, SheetData> LoadData()
    {
        var dataList = new Dictionary<long, SheetData>();

        string ListStr = null;
		int line = 0;
        TextAsset csvFile = Resources.Load<TextAsset>($"CSV/MEMCSV/{this.GetType().Name}");
        try
		{            
            string csvContent = csvFile.text;
            string[] lines = Regex.Split(csvContent, @"\r?\n");

            for (int i = 3; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                    continue;

                string[] values = CSVParser.Parse(lines[i].Trim());


                line = i;

                ExecutionData data = new ExecutionData();

                
				if(values[0] == "")
				    data.index = default;
				else
				    data.index = Convert.ToInt64(values[0]);
				
				if(values[3] == "")
				    data.executionType = default;
				else
				    data.executionType = (SystemEnum.eExecutionType)Enum.Parse(typeof(SystemEnum.eExecutionType), values[3]);
				
				if(values[5] == "")
				    data.targetStat = default;
				else
				    data.targetStat = (SystemEnum.eStats)Enum.Parse(typeof(SystemEnum.eStats), values[5]);
				
				if(values[6] == "")
				    data.executionPhase = default;
				else
				    data.executionPhase = (SystemEnum.eExecutionPhase)Enum.Parse(typeof(SystemEnum.eExecutionPhase), values[6]);
				
				if(values[7] == "")
				    data.executionDuration = default;
				else
				    data.executionDuration = Convert.ToInt32(values[7]);
				
				if(values[8] == "")
				    data.prob = default;
				else
				    data.prob = Convert.ToInt32(values[8]);
				
				if(values[9] == "")
				    data.input1 = default;
				else
				    data.input1 = Convert.ToInt64(values[9]);
				
				if(values[10] == "")
				    data.input2 = default;
				else
				    data.input2 = Convert.ToInt64(values[10]);
				
				if(values[11] == "")
				    data.input3 = default;
				else
				    data.input3 = Convert.ToInt64(values[11]);
				
				if(values[12] == "")
				    data.input4 = default;
				else
				    data.input4 = Convert.ToInt64(values[12]);
				
				if(values[13] == "")
				    data.nextCondition = default;
				else
				    data.nextCondition = Convert.ToInt64(values[13]);
				
				ListStr = values[14].Replace('[',' ');
				ListStr = ListStr.Replace(']', ' ');
				var nextExecutionListData = ListStr.ToString().Split(',').Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)).Select(x => Convert.ToInt64(x)).ToList();
				data.nextExecutionList = nextExecutionListData;
				

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