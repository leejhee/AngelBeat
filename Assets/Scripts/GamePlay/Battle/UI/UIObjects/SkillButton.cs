using GamePlay.Skill;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static Core.Foundation.Define.SystemString;

namespace AngelBeat
{
    public class SkillButton : Button
    {
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text skillName;
        
        public void SetButton(SkillModel model)
        {
            icon.sprite = ResourceManager.Instance.Load<Sprite>(new StringBuilder(SkillIconPath)
                .Append(model.Icon)
                .ToString());
            skillName.SetText(model.SkillName);
        }
    }
}

