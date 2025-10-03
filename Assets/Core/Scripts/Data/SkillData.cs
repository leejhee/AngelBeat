using Core.Scripts.Foundation.Define;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Data;
using System.Linq;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Core.Scripts.Data
{
    public partial class SkillData : SheetData
    {
public long index; // 스킬 ID
		public long characterID; // 스킬 사용 캐릭 ID
		public string skillName; // 스킬 이름
		
		public SystemEnum.eSkillType skillType; // 스킬 종류
		
		public SystemEnum.ePivot skillPivot; // 스킬 중심
		public long skillDamage; // 스킬 데미지
		public long skillRangeID; // 스킬 사용 사거리
		public long executionIndex; // 스킬 효과
		public int skillCritical; // 치명타 보정치
		public int skillAccuracy; // 명중율
		public string skillIconImage; // 스킬 아이콘명
		public string skillTimeLine; // 스킬 타임라인명
		
		public SystemEnum.eSkillUnlock unlockCondition; // 스킬 해금 조건
		
        /// <summary>Addressable(RM)로 CSV를 비동기 로드해 파싱함</summary>
        public override UniTask<Dictionary<long, SheetData>> ParseAsync(string csv, CancellationToken ct = default)
        {
            var dataList = new Dictionary<long, SheetData>();
            string ListStr = null;
            int line = 0;

            try
            { 
                string[] lines = csv.Split('\n');

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
					
					if(values[1] == "")
					    data.characterID = default;
					else
					    data.characterID = Convert.ToInt64(values[1]);
					
					if(values[3] == "")
					    data.skillName = default;
					else
					    data.skillName = Convert.ToString(values[3]);
					
					if(values[4] == "")
					    data.skillType = default;
					else
					    data.skillType = (SystemEnum.eSkillType)Enum.Parse(typeof(SystemEnum.eSkillType), values[4]);
					
					if(values[5] == "")
					    data.skillPivot = default;
					else
					    data.skillPivot = (SystemEnum.ePivot)Enum.Parse(typeof(SystemEnum.ePivot), values[5]);
					
					if(values[6] == "")
					    data.skillDamage = default;
					else
					    data.skillDamage = Convert.ToInt64(values[6]);
					
					if(values[7] == "")
					    data.skillRangeID = default;
					else
					    data.skillRangeID = Convert.ToInt64(values[7]);
					
					if(values[8] == "")
					    data.executionIndex = default;
					else
					    data.executionIndex = Convert.ToInt64(values[8]);
					
					if(values[9] == "")
					    data.skillCritical = default;
					else
					    data.skillCritical = Convert.ToInt32(values[9]);
					
					if(values[10] == "")
					    data.skillAccuracy = default;
					else
					    data.skillAccuracy = Convert.ToInt32(values[10]);
					
					if(values[11] == "")
					    data.skillIconImage = default;
					else
					    data.skillIconImage = Convert.ToString(values[11]);
					
					if(values[12] == "")
					    data.skillTimeLine = default;
					else
					    data.skillTimeLine = Convert.ToString(values[12]);
					
					if(values[13] == "")
					    data.unlockCondition = default;
					else
					    data.unlockCondition = (SystemEnum.eSkillUnlock)Enum.Parse(typeof(SystemEnum.eSkillUnlock), values[13]);
					

                    dataList[data.index] = data;
                }

                return UniTask.FromResult(dataList);
            }
            catch (Exception e)
            {
                Debug.LogError($"{this.GetType().Name}의 {line} 전후로 데이터 문제 발생: {e}");
                return UniTask.FromResult(new Dictionary<long, SheetData>());
            }
        }       
       
    }
}