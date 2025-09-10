using GamePlay.Battle;
using GamePlay.Features.Scripts.Battle;
using GamePlay.Features.Scripts.Battle.Unit;
using GamePlay.Skill;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AngelBeat
{
    public class SkillButtonPanel : MonoBehaviour
    {
        [SerializeField] private List<SkillButton> skillButtons;
    
        public void SetSkillButtons(CharBase focus, IReadOnlyList<SkillModel> skillList)
        {
            int skillCount = skillList.Count;
            for (int i = 0; i < skillButtons.Count; i++)
            {
                bool isSkill = i < skillCount;
                skillButtons[i].gameObject.SetActive(isSkill);
                if (isSkill)
                {
                    int idx = i;
                    SkillButton button = skillButtons[idx];
                    button.SetButton(skillList[idx]);
                    
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() =>
                    {
                        BattleController.Instance.ShowSkillPreview(skillList[idx]);
                        Debug.Log($"Skill {skillList[idx].SkillName} Selected");
                    });
                    
                }
            }
        }
    } 
}

