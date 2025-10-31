using Core.Scripts.Data;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using DataManager = Core.Scripts.Managers.DataManager;

namespace GamePlay.Features.Battle.Scripts.UI.UIObjects
{
    public class RewardObject : ToggleButton
    {
        //[SerializeField] private TMP_Text rewardText;
        [SerializeField] private Image rewardImage;
        [SerializeField] private Button interactionButton;

        [SerializeField] private int slotIndex;

        public Button InterActionButton => interactionButton;
        
        public event Action<int> Selected;
        public event Action<int> Deselected;
        
        public void SetReward(int idx, Sprite deselected = null, Sprite selected = null)
        {
            isSelected = false;
            slotIndex = idx;
            selectedFrame =  selected;
            nonSelectedFrame = deselected;
            
            
            Frame.sprite = deselected;
        }


        public override void OnSelect()
        {
            Debug.Log($"{slotIndex} 번 보상 선택");
            Selected?.Invoke(slotIndex);
        }

        public override void OnDeselect()
        {
            Debug.Log($"{slotIndex} 번 해제");
            Deselected?.Invoke(slotIndex);
        }
    }
}