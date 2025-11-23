using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Core.Scripts.Managers;

namespace GamePlay.Features.Battle.Scripts.UI.CharacterInfoPopup
{
    public class CharacterInfoPopupSkill : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image skillImage;
        [SerializeField] private Image frameImage;
        [SerializeField] private SkillDescription skillDescription;
        [SerializeField] private Sprite selectedFrame;
        [SerializeField] private Sprite nonSelectedFrame;
        [SerializeField] private Sprite inActiveFrame;

        [SerializeField] private GameObject IconObject;
        public async void SetSkillImage(CharacterInfoPresenter.InfoPopupSkillResourceRoot skillResourceRoot)
        {
            Sprite icon = await ResourceManager.Instance.LoadAsync<Sprite>(skillResourceRoot.IconRoot);
            Sprite tooltip = await ResourceManager.Instance.LoadAsync<Sprite>(skillResourceRoot.TooltipRoot);
            
            
            IconObject.SetActive(true);
            skillImage.sprite = icon;
            skillDescription.SetSkillDescription(tooltip);
            frameImage.sprite = skillResourceRoot.IsUsing ? selectedFrame : nonSelectedFrame;

        }

        public void InactiveSkillImage()
        {
            IconObject.SetActive(false);
            skillDescription = null;
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
