using GamePlay.Common.Scripts.Entities.Skills;
using GamePlay.Skill;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay.Features.Battle.Scripts.UI.CharacterInfoPopup
{
    public class SkillPanel: MonoBehaviour
    {
        [SerializeField] private List<CharacterInfoPopupSkill> skillList = new();

        public void SetSkills(IReadOnlyList<SkillModel> skills)
        {
            // 스킬은 무조건 7개씩 있다는 보장은 있지만 나중에 무결성 체크 해줄것
            int idx = 0;
            foreach (var skill in skills)
            {
                // 해금 했고, 선택한 스킬일 경우
                // 해금 했지만 선택하지 않은 스킬일 경우
                // 해금되지 않은 스킬일 경우
                skillList[idx].SetSkillImage(skill);
                idx++;
            }
        }
    }
}
