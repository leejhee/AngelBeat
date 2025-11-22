using GamePlay.Common.Scripts.Entities.Skills;
using GamePlay.Features.Battle.Scripts;
using GamePlay.Features.Battle.Scripts.UI.UIObjects;
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
        
        // 어떻게 하든 상관은 없을거같음.
        public int SlotIndex { get; private set; }
        public void BindSlot(int idx) => SlotIndex = idx;
        public event Action<int> Selected;
        public event Action<int> Deselected;
        
        private void Start()
        {
            isSelected = false;
        }
        
        public void SetButton(CharacterHUD.SkillInfo info)
        {
            this.GetComponent<Image>().sprite = nonSelectedFrame;
            
            icon.gameObject.SetActive(true);
            icon.sprite = info.skillIcon;
            icon.color = Color.white;
            
            this.GetComponent<Button>().interactable = true;
            selectable = true;
            skillDescription.SetSkillDescription(info.skillDescription);
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (selectable)
                skillDescription.gameObject.SetActive(true);
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            if (selectable)
                skillDescription.gameObject.SetActive(false);
        }
        
        public override void OnSelect()
        {
            Debug.Log($"스킬 선택 - {skillName}");
            Selected?.Invoke(SlotIndex);
        }

        public override void OnDeselect()
        {
            Debug.Log("스킬 해제");
            Deselected?.Invoke(SlotIndex);
        }
    }
}

