using GamePlay.Common.Scripts.Entities.Skills;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay.Features.Battle.Scripts.UI.CharacterInfoPopup
{
    public class SkillPanel: MonoBehaviour
    {
        [SerializeField] private List<CharacterInfoPopupSkill> skillList = new();

        public void SetSkills(
            List<string> activeSkills, 
            List<string> usingSkills)
        {
            
        }
    }
}
