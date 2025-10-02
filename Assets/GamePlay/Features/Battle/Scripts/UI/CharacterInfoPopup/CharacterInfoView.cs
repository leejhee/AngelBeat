using Character;
using Cysharp.Threading.Tasks;
using System.Threading;
using UIs.Runtime;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GamePlay.Features.Battle.Scripts.UI.CharacterInfoPopup
{
    public class CharacterInfoView : MonoBehaviour, IView
    {

        public GameObject Root { get; }
    
        [SerializeField] private PortraitPanel portraitPanel;
        [SerializeField] private PassivePanel passivePanel;
        [SerializeField] private SkillPanel skillPanel;
        [SerializeField] private StatPanel statPanel;
        [SerializeField] private EssencePanel essencePanel;
        public PortraitPanel PortraitPanel => portraitPanel;
        public PassivePanel PassivePanel => passivePanel;
        public SkillPanel SkillPanel => skillPanel;
        public StatPanel StatPanel => statPanel;
        public EssencePanel EssencePanel => essencePanel;
    
        [SerializeField] private Button leftButton;
        [SerializeField] private Button rightButton;
        [SerializeField] private Button closeButton;
        public Button LeftButton => leftButton;
        public Button RightButton => rightButton;
        public Button CloseButton => closeButton;
        
        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);

        public UniTask PlayEnterAsync(CancellationToken ct) => UniTask.CompletedTask;
        public UniTask PlayExitAsync(CancellationToken ct) => UniTask.CompletedTask;
    }

    public class CharacterInfoPresenter : PresenterBase<CharacterInfoView>
    {
        public CharacterInfoPresenter(IView view) : base(view) { }

        public override UniTask EnterAction(CancellationToken token)
        {
            SetCharacterInfoPopup(BattleController.Instance.FocusChar.CharInfo);
            #region Model To View

            #endregion

            #region View To Model

            ViewEvents.Subscribe(
                act => View.LeftButton.onClick.AddListener(new UnityAction(act)),
                act => View.LeftButton.onClick.RemoveAllListeners(),
                OnClickLeftButton
            );
            ViewEvents.Subscribe(
                act => View.RightButton.onClick.AddListener(new UnityAction(act)),
                act => View.RightButton.onClick.RemoveAllListeners(),
                OnClickRightButton
            );
            ViewEvents.Subscribe(
                act => View.CloseButton.onClick.AddListener(new UnityAction(act)),
                act => View.CloseButton.onClick.RemoveAllListeners(),
                View.Hide
            );
            #endregion
            
            return UniTask.CompletedTask;
        }


        private void SetCharacterInfoPopup(CharacterModel model)
        {
            View.PortraitPanel.SetPortraitPanel(model);
            View.PassivePanel.SetPassivePanel(model);
            View.SkillPanel.SetSkills(model.Skills);
            View.StatPanel.SetStats(model);
            View.EssencePanel.SetEssence(model);
        }

        private void OnClickLeftButton()
        {
            //List<CharacterModel> characters = BattleContoller.Instance.
        }

        private void OnClickRightButton()
        {
            
        }
        
    }
}