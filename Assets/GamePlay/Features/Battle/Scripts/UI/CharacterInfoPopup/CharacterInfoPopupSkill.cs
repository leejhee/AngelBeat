using GamePlay.Skill;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GamePlay.Features.Battle.Scripts.UI.CharacterInfoPopup
{
    public class CharacterInfoPopupSkill : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image skillImage;
        [SerializeField] private SkillDescription skillDescription;
        
        
        public void SetSkillImage(SkillModel model)
        {
            skillImage.sprite = model.Icon;
            skillDescription.SetSkillDescription(model);
            // 스킬 설명 추가
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            skillDescription.gameObject.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            skillDescription.gameObject.SetActive(false);
        }
    }
}
