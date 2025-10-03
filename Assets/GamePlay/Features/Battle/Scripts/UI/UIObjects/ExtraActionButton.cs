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
    }
}
