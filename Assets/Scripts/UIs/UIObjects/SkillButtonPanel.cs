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
                    skillButtons[i].SetButton(skillList[i]);
                    
                    skillButtons[i].onClick.RemoveAllListeners();
                    
                    
                    
                    //TODO : 스킬 미리보기 UI로 바뀌게 수정할 것.
                    //var idx = i;
                    //skillButtons[i].onClick.AddListener(() =>
                    //{
                    //    focus.SkillInfo.PlaySkill(skillList[idx].index, new SkillParameter());
                    //});
                    
                }
            }
        }
    } 
}

