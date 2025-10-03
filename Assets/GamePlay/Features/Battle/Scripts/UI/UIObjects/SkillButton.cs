using GamePlay.Common.Scripts.Entities.Skills;
using GamePlay.Features.Battle.Scripts;
using GamePlay.Features.Battle.Scripts.UI.UIObjects;
using GamePlay.Skill;
using System;
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
    public class SkillButton : ToggleButton, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text skillName;
        [SerializeField] private SkillDescription skillDescription;

        private void Start()
        {
            isSelected = false;
        }

        public void SetButton(SkillModel model)
        {
            // TODO: 튜토
            //icon.sprite = model.icon;
            //skillName.SetText(model.SkillName);
            
            
            skillDescription.SetSkillDescription(model);
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (skillDescription != null)
                skillDescription.gameObject.SetActive(true);
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            if (skillDescription != null)
                skillDescription.gameObject.SetActive(false);
        }

        public void OnClickSkillButton(int idx)
        {
            // if (!isSelected)
            // {
            //     frame.sprite = selectedFrame;
            //     isSelected = true;
            // }
            // else
            // {
            //     frame.sprite = nonSelectedFrame;
            //     isSelected = false;
            // }

            
            var skills = BattleController.Instance.FocusChar.CharInfo.Skills;
            if (idx >= skills.Count) return;
            SkillModel model = skills[idx];
            BattleController.Instance.ShowSkillPreview(model);
            

        }
        
    }
}

