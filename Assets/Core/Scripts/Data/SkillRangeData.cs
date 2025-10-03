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
    public partial class SkillRangeData : SheetData
    {
public long index; // 스킬 범위 ID
		
		public SystemEnum.eSkillOwner skillOwner; // 스킬 주체
		public long skillIndex; // 스킬ID
		
		public SystemEnum.eSkillType skillType; // 스킬 종류
		
		public SystemEnum.ePivot skillPivot; // 스킬 중심
		public int skillPivotRange; // 스킬 중심거리
		public bool Origin; // 자기 자리
		public int Forward; // 앞
		public int Backward; // 뒤
		public bool Up; // 위
		public int UpForward; // 위앞
		public int UpBackward; // 위뒤
		public bool Down; // 아래
		public int DownForward; // 아래앞
		public int DownBackward; // 아래뒤
		
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

                    SkillRangeData data = new SkillRangeData();

                    
					if(values[0] == "")
					    data.index = default;
					else
					    data.index = Convert.ToInt64(values[0]);
					
					if(values[1] == "")
					    data.skillOwner = default;
					else
					    data.skillOwner = (SystemEnum.eSkillOwner)Enum.Parse(typeof(SystemEnum.eSkillOwner), values[1]);
					
					if(values[2] == "")
					    data.skillIndex = default;
					else
					    data.skillIndex = Convert.ToInt64(values[2]);
					
					if(values[4] == "")
					    data.skillType = default;
					else
					    data.skillType = (SystemEnum.eSkillType)Enum.Parse(typeof(SystemEnum.eSkillType), values[4]);
					
					if(values[5] == "")
					    data.skillPivot = default;
					else
					    data.skillPivot = (SystemEnum.ePivot)Enum.Parse(typeof(SystemEnum.ePivot), values[5]);
					
					if(values[6] == "")
					    data.skillPivotRange = default;
					else
					    data.skillPivotRange = Convert.ToInt32(values[6]);
					
					if(values[7] == "")
					    data.Origin = default;
					else
					    data.Origin = Convert.ToBoolean(values[7].ToLowerInvariant());
					
					if(values[8] == "")
					    data.Forward = default;
					else
					    data.Forward = Convert.ToInt32(values[8]);
					
					if(values[9] == "")
					    data.Backward = default;
					else
					    data.Backward = Convert.ToInt32(values[9]);
					
					if(values[10] == "")
					    data.Up = default;
					else
					    data.Up = Convert.ToBoolean(values[10].ToLowerInvariant());
					
					if(values[11] == "")
					    data.UpForward = default;
					else
					    data.UpForward = Convert.ToInt32(values[11]);
					
					if(values[12] == "")
					    data.UpBackward = default;
					else
					    data.UpBackward = Convert.ToInt32(values[12]);
					
					if(values[13] == "")
					    data.Down = default;
					else
					    data.Down = Convert.ToBoolean(values[13].ToLowerInvariant());
					
					if(values[14] == "")
					    data.DownForward = default;
					else
					    data.DownForward = Convert.ToInt32(values[14]);
					
					if(values[15] == "")
					    data.DownBackward = default;
					else
					    data.DownBackward = Convert.ToInt32(values[15]);
					

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