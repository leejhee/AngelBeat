using System.Collections.Generic;
using UnityEngine;

namespace AngelBeat
{
    public class SkillButtonPanel : MonoBehaviour
    {
        [SerializeField] private List<SkillButton> skillButtons;
        public List<SkillButton> SkillButtons => skillButtons;
    } 
}

