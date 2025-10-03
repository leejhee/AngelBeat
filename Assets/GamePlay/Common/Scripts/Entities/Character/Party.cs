using Core.Scripts.Foundation.Define;
using GamePlay.Common.Scripts.Entities.Character;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace GamePlay.Entities.Scripts.Character
{
    [Serializable]
    public class Party
    {
        [Header("어느 타입의 파티인가요?")]
        public SystemEnum.eCharType partyType;

        [Header("파티 멤버들을 기록합니다.")]
        public List<CharacterModel> partyMembers;

        [Header("해당 파티 전원에 적용되는 효과를 기록합니다.")]
        public List<long> FunctionsPerParty;

        public Party(
            List<CharacterModel> partyMembers,
            List<long> FunctionsPerParty=null,
            SystemEnum.eCharType partyType = SystemEnum.eCharType.Player
            )
        {
            this.partyMembers = partyMembers;
            this.FunctionsPerParty = FunctionsPerParty;
            this.partyType = partyType;
        }
        
        public void AddMember(CharacterModel member)
        {
            partyMembers.Add(member);
        }

        public override string ToString()
        {
            string func = FunctionsPerParty == null || FunctionsPerParty.Count == 0
                ? "없음" : $"{FunctionsPerParty.Count}개 버프 있음";
            return new StringBuilder($"{partyType} : {partyMembers.Count}명 | 버프 : ").Append(func).ToString();
        }
        
        //업데이트에 안쓰니까 마음놓고 쓰도록 하자.
        public CharacterModel SearchCharacter(string charName)
        {
            return partyMembers.Find(x => x.Name == charName);
        }
    }
}