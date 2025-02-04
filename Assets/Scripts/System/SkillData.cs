using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[TODO] : 데이터 정상화 시 파싱해서 사용할 것.
public class SkillData: SheetData
{
    public long index;
    // 밑에는 그 외 데이터에서 파싱한 내용들이 들어가 있어야 한다.
    public override Dictionary<long, SheetData> LoadData()
    {
        throw new System.NotImplementedException();
    }
}
