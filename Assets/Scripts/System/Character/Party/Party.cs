using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using static SystemEnum;

/// <summary> 파티에는 이 정보가 저장됩니다. 몬스터도 예외 없을지도? </summary>
[Serializable]
public class LightWeightCharacter
{
    private long _index;
    private CharStat _stat;

    public long Index => _index;
    public CharStat Stat => _stat;

    public LightWeightCharacter(long index, CharStat stat)
    {
        _index = index;
        _stat = stat;
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
