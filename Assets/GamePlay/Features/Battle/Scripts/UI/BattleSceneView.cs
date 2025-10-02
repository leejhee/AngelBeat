using Character;
using Core.Scripts.Foundation.Define;
using Core.Scripts.Managers;
using Cysharp.Threading.Tasks;
using GamePlay.Features.Battle.Scripts.Models;
using GamePlay.Features.Battle.Scripts.UI.UIObjects;
using System.Threading;
using UIs.Runtime;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GamePlay.Features.Battle.Scripts.UI
{
    public class BattleHUDView : MonoBehaviour, IView
    {
        public GameObject Root { get; }
        
        [SerializeField] private Button turnEndButton;
        [SerializeField] private Button menuButton;
        [SerializeField] private CharacterHUD characterHUD;
        [SerializeField] private GameObject turnObjectParent;

        public Button TurnEndButton => turnEndButton;
        public Button MenuButton => menuButton;
        public CharacterHUD CharacterHUD => characterHUD;
        public GameObject TurnObjectParent => turnObjectParent;
        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);

        public UniTask PlayEnterAsync(CancellationToken ct) => UniTask.CompletedTask;
        public UniTask PlayExitAsync(CancellationToken ct) => UniTask.CompletedTask;
        

        public void ChangeHp(int delta)
        {
            characterHUD.ReduceHpUI(delta);
        }
    }

    public class BattleHUDPresenter : PresenterBase<BattleHUDView>
    {
        #region AddressablesDefinition
        private readonly string _turnPortraitAddress = "TurnPortrait";
        #endregion
        
        public BattleHUDPresenter(IView view) : base(view) { }

        public override UniTask EnterAction(CancellationToken token)
        {
            #region Model Events

            //hp 구독
            ModelEvents.Subscribe<HPModel>(
                act => BattleController.Instance.FocusChar.CharStat.OnHPChanged += act,
                act => BattleController.Instance.FocusChar.CharStat.OnHPChanged -= act,
                OnHPChanged);
            // 턴 바뀜 이벤트 구독
            // ModelEvents.Subscribe<CharacterModel>(
            //     action => BattleController.
            //     action =>
            //     OnTurnChanged
            //         );
            #endregion

            #region View Events
            
            // 턴 종료 버튼 구독
            ViewEvents.Subscribe(
                act => View.TurnEndButton.onClick.AddListener(new UnityAction(act)),
                act => View.TurnEndButton.onClick.RemoveAllListeners(),
                OnClickTurnEndButton
            );
            
            // 메뉴 버튼 구독
            ViewEvents.Subscribe(
                act => View.MenuButton.onClick.AddListener(new UnityAction(act)),
                act => View.MenuButton.onClick.RemoveAllListeners(),
                OnClickMenuButton
            );
            // 캐릭터 초상화 꾹 누르기
            // TODO: 나중에 꾹 누르기로 바꾸기 일단은 한번 클릭으로 설정
            ViewEvents.Subscribe(
                act => View.CharacterHUD.CharacterPortraitButton.onClick.AddListener(new UnityAction(act)),
                act => View.CharacterHUD.CharacterPortraitButton.onClick.RemoveAllListeners(),
                OnClickPortraitButton
            );

            ViewEvents.Subscribe(
                act => View.CharacterHUD.JumpButton.onClick.AddListener(new UnityAction(act)),
                act => View.CharacterHUD.JumpButton.onClick.RemoveAllListeners(),
                OnClickJumpButton
            );
            ViewEvents.Subscribe(
                act => View.CharacterHUD.PushButton.onClick.AddListener(new UnityAction(act)),
                act => View.CharacterHUD.PushButton.onClick.RemoveAllListeners(),
                OnClickPushButton
                );
            ViewEvents.Subscribe(
                act => View.CharacterHUD.InvenButton.onClick.AddListener(new UnityAction(act)),
                act => View.CharacterHUD.InvenButton.onClick.RemoveAllListeners(),
                OnClickInvenButton
                );
            
            // for (int i = 0; i < 4; i++)
            // {
            //     ViewEvents.Subscribe(
            //         act => View.CharacterHUD.SkillPanel.SkillButtons[i].GetComponent<Button>().onClick.AddListener(new UnityAction(act)),
            //         act=> View.CharacterHUD.SkillPanel.SkillButtons[i].GetComponent<Button>().onClick.RemoveAllListeners(),
            //         BattleController.Instance.ShowSkillPreview()
            //     );
            // }
            #endregion
            return UniTask.CompletedTask;
        }


        #region Model To View
        private void OnHPChanged(HPModel model)
        {
            int delta = model.Delta;
            View.ChangeHp(delta);
        }

        private void OnAPChanged(int delta)
        {
            
        }
        public void OnTurnChanged(BattleController.TurnModel model)
        {
            
            // 턴 HUD 초상화 생성
            async UniTask InstantiatePortrait(Sprite sprite)
            {
                GameObject go = await ResourceManager.Instance.InstantiateAsync(_turnPortraitAddress, View.TurnObjectParent.transform);
                go.GetComponent<Image>().sprite = sprite;
            }
        }

        private void OnCharacterHUDOpen(CharacterModel model)
        {
            // 초상화
            //Sprite charPortrait = model.
            // 이름
            string name = model.Name;
            // 현재 체력
            long curHp = model.Stat.GetStat(SystemEnum.eStats.NHP);
            long maxHp = model.Stat.GetStat(SystemEnum.eStats.NMHP);
            
            
            // 현재 액션포인트
            long curAp = model.Stat.GetStat(SystemEnum.eStats.NACTION_POINT);
            long maxAp = model.Stat.GetStat(SystemEnum.eStats.NMACTION_POINT);
            
            
            
            // HUD 패널 오픈
            View.CharacterHUD.gameObject.SetActive(true);
            // 체력, 액션포인트, 초상화 설정
            View.CharacterHUD.ShowCharacterHUD(name, curHp, maxHp, curAp, maxAp);
            // 스킬 버튼 생성
            View.CharacterHUD.SetSkillButtons(model.Skills);
            
        }
        #endregion

        #region View To Model

        private void OnClickTurnEndButton()
        {
            Debug.Log("턴 종료");
        }

        private void OnClickMenuButton()
        {
            Debug.Log("메뉴창 열기");
        }

        private void OnClickPortraitButton()
        {
            Debug.Log("캐릭터 정보창 띄우기");
        }

        #region ExtraActions

        private void OnClickJumpButton()
        {
            Debug.Log("점프");
        }

        private void OnClickPushButton()
        {
            Debug.Log("밀기");
        }

        private void OnClickInvenButton()
        {
            Debug.Log("인벤토리 오픈");
        }
        #endregion
        
        #endregion

    }
}



