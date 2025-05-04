using System;
using System.Collections.Generic;
using UnityEngine;
// ReSharper disable All

namespace AngelBeat.Core.Character
{
    [Serializable]
    public class Party
    {
        [Header("어느 타입의 파티인가요?")]
        public SystemEnum.eCharType partyType;

        [Header("파티 멤버들을 기록합니다.")]
        public List<CharacterInfo> partyMembers;

        [Header("해당 파티 전원에 적용되는 효과를 기록합니다.")]
        public List<long> FunctionsPerParty;
        
    }
}