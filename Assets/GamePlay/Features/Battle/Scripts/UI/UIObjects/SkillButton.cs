using GamePlay.Common.Scripts.Entities.Skills;
using GamePlay.Skill;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using static Core.Scripts.Foundation.Define.SystemString;

namespace AngelBeat
{
    public class SkillButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image frame;
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text skillName;
        [SerializeField] private SkillDescription skillDescription;
        [SerializeField] private Sprite selectedFrame;
        [SerializeField] private Sprite nonSelectedFrame;
        public void SetButton(SkillModel model)
        {
            icon.sprite = model.Icon;
            ///skillName.SetText(model.SkillName);
            skillDescription.SetSkillDescription(model);
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            skillDescription.gameObject.SetActive(true);
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            skillDescription.gameObject.SetActive(false);
        }

        public void OnClickSkillButton()
        {
            SelectSkillButton(true);
        }
        
        public void SelectSkillButton(bool selected)
        {
            if (selected)
            {
                frame.sprite = selectedFrame;
            }
            else
            {
                frame.sprite = nonSelectedFrame;
            }
        }
    }
}

