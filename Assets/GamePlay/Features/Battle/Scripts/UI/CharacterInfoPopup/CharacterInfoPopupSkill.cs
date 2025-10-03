using GamePlay.Common.Scripts.Entities.Skills;
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
        [SerializeField] private Sprite skillIcon;
        [SerializeField] private Sprite selectedFrame;
        [SerializeField] private Sprite nonSelectedFrame;
        
        public void SetSkillImage(SkillModel model)
        {
            skillImage.sprite = model.Icon;
            skillDescription.SetSkillDescription(model);
            // 스킬 설명 추가
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
    }
}
