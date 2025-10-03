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
    public partial class CompanionData : SheetData
    {
public long index; // 동료 ID
		public string companionName; // 캐릭터 이름(추후 stringcode로 교체할 것)
		public long charStatID; // 캐릭터 스탯 ID
		public long PassiveD1; // 첫 번째 패시브
		public long PassiveD2; // 두 번째 패시브
		public long Skill1ID; // 첫 번째 스킬
		public long Skill2ID; // 두 번째 스킬
		public long Skill3ID; // 세 번째 스킬
		public long Skill4ID; // 네 번째 스킬
		public long Skill5ID; // 다섯 번째 스킬
		public long Skill6ID; // 여섯 번째 스킬
		public long Skill7ID; // 일곱 번째 스킬
		
		public SystemEnum.eUnlockCondition UnlockCondition; // 해금조건
		
		public SystemEnum.eUpgradeLevel UpgradeLevel; // 강화단계
		public string charPrefabName; // 캐릭터 프리팹 루트
		public string charImage; // 캐릭터 아이콘 루트
		public string charLDRoute; // 캐릭터 LD 루트
		
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

                    CompanionData data = new CompanionData();

                    
					if(values[0] == "")
					    data.index = default;
					else
					    data.index = Convert.ToInt64(values[0]);
					
					if(values[2] == "")
					    data.companionName = default;
					else
					    data.companionName = Convert.ToString(values[2]);
					
					if(values[3] == "")
					    data.charStatID = default;
					else
					    data.charStatID = Convert.ToInt64(values[3]);
					
					if(values[4] == "")
					    data.PassiveD1 = default;
					else
					    data.PassiveD1 = Convert.ToInt64(values[4]);
					
					if(values[5] == "")
					    data.PassiveD2 = default;
					else
					    data.PassiveD2 = Convert.ToInt64(values[5]);
					
					if(values[6] == "")
					    data.Skill1ID = default;
					else
					    data.Skill1ID = Convert.ToInt64(values[6]);
					
					if(values[7] == "")
					    data.Skill2ID = default;
					else
					    data.Skill2ID = Convert.ToInt64(values[7]);
					
					if(values[8] == "")
					    data.Skill3ID = default;
					else
					    data.Skill3ID = Convert.ToInt64(values[8]);
					
					if(values[9] == "")
					    data.Skill4ID = default;
					else
					    data.Skill4ID = Convert.ToInt64(values[9]);
					
					if(values[10] == "")
					    data.Skill5ID = default;
					else
					    data.Skill5ID = Convert.ToInt64(values[10]);
					
					if(values[11] == "")
					    data.Skill6ID = default;
					else
					    data.Skill6ID = Convert.ToInt64(values[11]);
					
					if(values[12] == "")
					    data.Skill7ID = default;
					else
					    data.Skill7ID = Convert.ToInt64(values[12]);
					
					if(values[13] == "")
					    data.UnlockCondition = default;
					else
					    data.UnlockCondition = (SystemEnum.eUnlockCondition)Enum.Parse(typeof(SystemEnum.eUnlockCondition), values[13]);
					
					if(values[14] == "")
					    data.UpgradeLevel = default;
					else
					    data.UpgradeLevel = (SystemEnum.eUpgradeLevel)Enum.Parse(typeof(SystemEnum.eUpgradeLevel), values[14]);
					
					if(values[15] == "")
					    data.charPrefabName = default;
					else
					    data.charPrefabName = Convert.ToString(values[15]);
					
					if(values[16] == "")
					    data.charImage = default;
					else
					    data.charImage = Convert.ToString(values[16]);
					
					if(values[17] == "")
					    data.charLDRoute = default;
					else
					    data.charLDRoute = Convert.ToString(values[17]);
					

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