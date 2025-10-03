using System;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.Features.Battle.Scripts.UI.UIObjects
{
    public class ExtraActionButton : ToggleButton
    {
        
        [SerializeField] private Button actionButton;
        public Button ActionButton => actionButton;
        
        private void Start()
        {
            isSelected = false;
        }
        

        public bool isSelected { get; set; }
        public override void OnSelect()
        {
            
        }

        public override void OnDeselect()
        {
            // 여기서 버튼 해제 일단 해줌
            Debug.Log("추가행동 해제");
        }
    }
}
