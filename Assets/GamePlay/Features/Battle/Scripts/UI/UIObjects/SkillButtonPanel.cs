using GamePlay.Battle;
using GamePlay.Features.Battle.Scripts;
using GamePlay.Features.Battle.Scripts.Unit;
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
        public List<SkillButton> SkillButtons => skillButtons;
    } 
}

