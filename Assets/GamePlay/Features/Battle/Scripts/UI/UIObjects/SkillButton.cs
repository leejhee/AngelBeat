using GamePlay.Skill;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static Core.Scripts.Foundation.Define.SystemString;

namespace AngelBeat
{
    public class SkillButton : Button
    {
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text skillName;
        
        public void SetButton(SkillModel model)
        {
            icon.sprite = model.Icon;
            skillName.SetText(model.SkillName);
        }
    }
}

