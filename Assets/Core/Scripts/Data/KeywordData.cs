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
    public partial class KeywordData : SheetData
    {
public long index; // 키워드 ID
		
		public SystemEnum.eKeyword keywordType; // 키워드 enum
		public string keywordName; // 키워드 이름
		
		public SystemEnum.eInfluenceType InfluenceType; // 긍정/중립/부정
		public string keywordIcon; // 키워드 아이콘
		public long keywordExecution; // 키워드 효과(삭제예정)
		public bool iconIsVisible; // 아이콘이 보이는지
		
        /// <summary>Addressable(RM)로 CSV를 비동기 로드해 파싱함</summary>
        public override async UniTask<Dictionary<long, SheetData>> ParseAsync(string csv, CancellationToken ct = default)
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

                    KeywordData data = new KeywordData();

                    
					if(values[0] == "")
					    data.index = default;
					else
					    data.index = Convert.ToInt64(values[0]);
					
					if(values[2] == "")
					    data.keywordType = default;
					else
					    data.keywordType = (SystemEnum.eKeyword)Enum.Parse(typeof(SystemEnum.eKeyword), values[2]);
					
					if(values[3] == "")
					    data.keywordName = default;
					else
					    data.keywordName = Convert.ToString(values[3]);
					
					if(values[4] == "")
					    data.InfluenceType = default;
					else
					    data.InfluenceType = (SystemEnum.eInfluenceType)Enum.Parse(typeof(SystemEnum.eInfluenceType), values[4]);
					
					if(values[5] == "")
					    data.keywordIcon = default;
					else
					    data.keywordIcon = Convert.ToString(values[5]);
					
					if(values[6] == "")
					    data.keywordExecution = default;
					else
					    data.keywordExecution = Convert.ToInt64(values[6]);
					
					if(values[7] == "")
					    data.iconIsVisible = default;
					else
					    data.iconIsVisible = Convert.ToBoolean(values[7]);
					

                    dataList[data.index] = data;
                }

                return dataList;
            }
            catch (Exception e)
            {
                Debug.LogError($"{this.GetType().Name}의 {line} 전후로 데이터 문제 발생: {e}");
                return new Dictionary<long, SheetData>();
            }
        }       
       
    }
}