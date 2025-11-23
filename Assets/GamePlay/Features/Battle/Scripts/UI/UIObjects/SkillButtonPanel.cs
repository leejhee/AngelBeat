using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AngelBeat
{
    
    public class SkillButtonPanel : MonoBehaviour
    {
        [SerializeField] private List<SkillButton> skillButtons;
        public List<SkillButton> SkillButtons => skillButtons;
        
        public void SetInteractable(bool enable)
        {
            foreach (SkillButton skillButton in skillButtons)
            {
                Button button = skillButton.GetComponent<Button>();
                button.interactable = enable;
            }
        }
        
    } 
}

