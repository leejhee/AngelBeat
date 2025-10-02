using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.Features.Battle.Scripts.UI.UIObjects
{
    public class ExtraActionButton : MonoBehaviour
    {
        [SerializeField] private Sprite selectedFrame;
        [SerializeField] private Sprite nonSelectedFrame;
        [SerializeField] private Button actionButton;
        public Button ActionButton => actionButton;

        public void SelectExtraActionButton(bool isSelected)
        {
            if (isSelected)
            {
                actionButton.image.sprite = selectedFrame;
            }
            else
            {
                actionButton.image.sprite = nonSelectedFrame;
            }
        }

    }
}
