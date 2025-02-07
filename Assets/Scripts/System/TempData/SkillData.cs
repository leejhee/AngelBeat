using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[TODO] : 데이터 정상화 시 파싱해서 사용할 것.
public class SkillData: SheetData
{
    public long index;

    public long skillCondition;
    // => 스킬 사용 조건. 만족 못하면 UI 조정. 조건 계산기 필요.

    public eSkillType skillType;
    // => 스킬 종류. 

    public long skillSubjects;
    // => 스킬 클릭 후 

    public string skillTimeLineName;
    // => 타임라인을 이 이름으로 로드함. 스킬 시작시 여기서 재생

    /*//////스킬 사용 후, 회피 판정 필요//////*/
    public int skillAccuracy;
    // => 명중률. 회피 판정에 사용 // 

    /*//////스킬 사용 후 명중 판정 후 대미지 계산에 필요//////*/
    public int skillCritical;
    // => 치명타 배율. 치명타 계수에 합적용으로 계산 //

    public long skillDamage;
    // => 대미지 계산식 적용 인덱스 //

    public long executionIndex;
    // => 대미지와 별도로, 버프 또는 디버프 또는 스택 부여에 사용 //

    public override Dictionary<long, SheetData> LoadData()
    {
        return null;
    }
}

public enum eSkillType
{
    PhysicalAttack,
    Buff,
    Debuff,
    MagicAttack,
    eMax
}

public class DamageCalculator
{
    public static string[] polynomial;

    public static void PolyParser(string Input)
    {

    }

    public static int PolyCalculator()
    {
        return 0;
    }

}