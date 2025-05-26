using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

[Serializable]
public partial class SkillData : SheetData
{
    public long index; // 스킬 ID
	public string skillName; // 스킬 이름
	public SystemEnum.eSkillType skillType; // 스킬 종류
	public int skillRange; // 스킬 사용 사거리
	public SystemEnum.ePivot skillPivot; // 스킬 중심
	public int skillPivotRange; // 스킬 중심거리
	public int skillCritical; // 치명타 배율
	public int skillAccuracy; // 명중율
	public long skillDamage; // 스킬 데미지
	public long executionIndex; // 스킬 효과
	public string skillCondition; // 스킬 발동 조건
	public string skillIconImage; // 스킬 아이콘명
	public string skillTimeLine; // 스킬 타임라인명
	public SystemEnum.eSkillUnlock unlockCondition; // 스킬 해금 조건
	

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

                SkillData data = new SkillData();

                
				if(values[0] == "")
				    data.index = default;
				else
				    data.index = Convert.ToInt64(values[0]);
				
				if(values[2] == "")
				    data.skillName = default;
				else
				    data.skillName = Convert.ToString(values[2]);
				
				if(values[3] == "")
				    data.skillType = default;
				else
				    data.skillType = (SystemEnum.eSkillType)Enum.Parse(typeof(SystemEnum.eSkillType), values[3]);
				
				if(values[4] == "")
				    data.skillRange = default;
				else
				    data.skillRange = Convert.ToInt32(values[4]);
				
				if(values[5] == "")
				    data.skillPivot = default;
				else
				    data.skillPivot = (SystemEnum.ePivot)Enum.Parse(typeof(SystemEnum.ePivot), values[5]);
				
				if(values[6] == "")
				    data.skillPivotRange = default;
				else
				    data.skillPivotRange = Convert.ToInt32(values[6]);
				
				if(values[7] == "")
				    data.skillCritical = default;
				else
				    data.skillCritical = Convert.ToInt32(values[7]);
				
				if(values[8] == "")
				    data.skillAccuracy = default;
				else
				    data.skillAccuracy = Convert.ToInt32(values[8]);
				
				if(values[9] == "")
				    data.skillDamage = default;
				else
				    data.skillDamage = Convert.ToInt64(values[9]);
				
				if(values[10] == "")
				    data.executionIndex = default;
				else
				    data.executionIndex = Convert.ToInt64(values[10]);
				
				if(values[11] == "")
				    data.skillCondition = default;
				else
				    data.skillCondition = Convert.ToString(values[11]);
				
				if(values[12] == "")
				    data.skillIconImage = default;
				else
				    data.skillIconImage = Convert.ToString(values[12]);
				
				if(values[13] == "")
				    data.skillTimeLine = default;
				else
				    data.skillTimeLine = Convert.ToString(values[13]);
				
				if(values[14] == "")
				    data.unlockCondition = default;
				else
				    data.unlockCondition = (SystemEnum.eSkillUnlock)Enum.Parse(typeof(SystemEnum.eSkillUnlock), values[14]);
				

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