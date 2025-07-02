using AngelBeat;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Data;
using System.Linq;

namespace AngelBeat
{
    public partial class CharStatData : SheetData
    {
public long index; // 캐릭터 스탯 ID
		public int blue; // 청
		public int red; // 적
		public int yellow; // 황
		public int white; // 백
		public int black; // 흑
		public int HP; // 체력
		public int armor; // 물리방어력
		public int magicResist; // 마법방어력
		public int meleeAttack; // 물리공격력
		public int magicalAttack; // 마법공격력
		public int critChance; // 치명타율
		public int speed; // 행동속도
		public int actionPoint; // 이동력
		public int dodge; // 회피율
		public int resistance; // 상태이상저항
		public int rangeIncrease; // 사거리증가
		

        public override Dictionary<long, SheetData> LoadData()
        {
            var dataList = new Dictionary<long, SheetData>();

            string ListStr = null;
			int line = 0;
            TextAsset csvFile = Resources.Load<TextAsset>($"CSV/MEMCSV/{this.GetType().Name}");
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

                    CharStatData data = new CharStatData();

                    
					if(values[0] == "")
					    data.index = default;
					else
					    data.index = Convert.ToInt64(values[0]);
					
					if(values[1] == "")
					    data.blue = default;
					else
					    data.blue = Convert.ToInt32(values[1]);
					
					if(values[2] == "")
					    data.red = default;
					else
					    data.red = Convert.ToInt32(values[2]);
					
					if(values[3] == "")
					    data.yellow = default;
					else
					    data.yellow = Convert.ToInt32(values[3]);
					
					if(values[4] == "")
					    data.white = default;
					else
					    data.white = Convert.ToInt32(values[4]);
					
					if(values[5] == "")
					    data.black = default;
					else
					    data.black = Convert.ToInt32(values[5]);
					
					if(values[6] == "")
					    data.HP = default;
					else
					    data.HP = Convert.ToInt32(values[6]);
					
					if(values[7] == "")
					    data.armor = default;
					else
					    data.armor = Convert.ToInt32(values[7]);
					
					if(values[8] == "")
					    data.magicResist = default;
					else
					    data.magicResist = Convert.ToInt32(values[8]);
					
					if(values[9] == "")
					    data.meleeAttack = default;
					else
					    data.meleeAttack = Convert.ToInt32(values[9]);
					
					if(values[10] == "")
					    data.magicalAttack = default;
					else
					    data.magicalAttack = Convert.ToInt32(values[10]);
					
					if(values[11] == "")
					    data.critChance = default;
					else
					    data.critChance = Convert.ToInt32(values[11]);
					
					if(values[12] == "")
					    data.speed = default;
					else
					    data.speed = Convert.ToInt32(values[12]);
					
					if(values[13] == "")
					    data.actionPoint = default;
					else
					    data.actionPoint = Convert.ToInt32(values[13]);
					
					if(values[14] == "")
					    data.dodge = default;
					else
					    data.dodge = Convert.ToInt32(values[14]);
					
					if(values[15] == "")
					    data.resistance = default;
					else
					    data.resistance = Convert.ToInt32(values[15]);
					
					if(values[16] == "")
					    data.rangeIncrease = default;
					else
					    data.rangeIncrease = Convert.ToInt32(values[16]);
					

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
}