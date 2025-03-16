using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using static SystemEnum;

/// <summary> 파티에는 이 정보가 저장됩니다. 몬스터도 예외 없을지도? </summary>
[Serializable]
public class LightWeightCharacter
{
    private long _index;
    private CharData _data;
    private CharStat _stat;
    private Vector3 _curPos;

    public long Index => _index;
    public CharStat Stat => _stat;

    public LightWeightCharacter(long index)
    {
        _index = index;
        _data = DataManager.Instance.GetData<CharData>(index);
        if(_data == null)
        {
            Debug.LogError("생성자 중 포함되지 않은 캐릭터데이터에 의한 오류");
            return;
        }
        else
        {
            var stat = DataManager.Instance.GetData<CharStatData>(index);
            _stat = new CharStat(stat);
        }

        _curPos = default;
    }

    public LightWeightCharacter(long index, CharData data, CharStat stat, Vector3 curPos)
    {
        _index = index;
        _data = data;
        _stat = stat;
        _curPos = curPos;
    }
}


public class Party : ScriptableObject
{
    [Header("어느 타입의 파티인가요?")]
    public eCharType partyType;

    [Header("파티 멤버들을 기록합니다.")]
    public List<LightWeightCharacter> partyMembers;

    [Header("해당 파티 전원에 적용되는 효과를 기록합니다.")]
    public List<long> FunctionsPerParty;


}
