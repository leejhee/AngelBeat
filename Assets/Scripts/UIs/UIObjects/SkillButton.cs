using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static SystemString;

namespace AngelBeat
{
    public class SkillButton : Button
    {
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text skillName;
        
        public void SetButton(SkillData skillData)
        {
            icon.sprite = ResourceManager.Instance.Load<Sprite>(new StringBuilder(SkillIconPath)
                .Append(skillData.skillIconImage)
                .ToString());
            skillName.SetText(skillData.skillName);
        }
    }
}

