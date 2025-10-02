using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.Features.Battle.Scripts.UI.CharacterInfoPopup
{
    public class CharacterInfoPopupSkill : MonoBehaviour
    {
        [SerializeField] private Image skillImage;

        public void SetSkillImage(Sprite sprite)
        {
            skillImage.sprite = sprite;
        }
    }
}
