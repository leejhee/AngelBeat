using Character;
using Core.Scripts.Foundation.Define;
using Core.Scripts.Managers;
using Cysharp.Threading.Tasks;
using GamePlay.Features.Battle.Scripts.Models;
using GamePlay.Features.Battle.Scripts.Unit;
using System;
using System.Threading;
using TMPro;
using UIs.Runtime;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GamePlay.Features.Battle.Scripts.UI
{
    public class BattleHUDView : MonoBehaviour, IView
    {
        #region AddressablesDefinition
        private readonly string characterPortraitAddress = "CharacterPortrait";
        #endregion

        public GameObject Root { get; }

        [SerializeField]
        private GameObject turnObjectParent;

        [SerializeField] private Button turnEndButton;
        [SerializeField] private Button menuButton;
        [SerializeField] private CharacterHUD characterHUD;
        
        public Button TurnEndButton => turnEndButton;
        public Button MenuButton => menuButton;
        public CharacterHUD CharacterHUD => characterHUD;
        
        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);

        public UniTask PlayEnterAsync(CancellationToken ct) => UniTask.CompletedTask;
        public UniTask PlayExitAsync(CancellationToken ct) => UniTask.CompletedTask;
        
        public async void InstantiateCharacterPortrait()
        {
            try
            {
                await ResourceManager.Instance.InstantiateAsync(characterPortraitAddress, turnObjectParent.transform);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void ChangeHP(int delta)
        {
            characterHUD.ReduceHpUI(delta);
        }
    }

    public class BattleHUDPresenter : PresenterBase<BattleHUDView>
    {
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
            #endregion
            return UniTask.CompletedTask;
        }


        #region Model To View
        private void OnHPChanged(HPModel model)
        {
            int delta = model.Delta;
            View.ChangeHP(delta);
        }
        public void OnTurnChanged()
        {
            
        }

        public void OnCharacterHUDOpen(CharacterModel model)
        {
            // 초상화 변경
            //Sprite charPortrait = model.
            // 현재 체력
            long curHp = model.Stat.GetStat(SystemEnum.eStats.NHP);
            long maxHp = model.Stat.GetStat(SystemEnum.eStats.NMHP);
            // 현재 액션포인트
            long curAp = model.Stat.GetStat(SystemEnum.eStats.NACTION_POINT);
            long maxAp = model.Stat.GetStat(SystemEnum.eStats.NMACTION_POINT);
            
            // HUD 패널 오픈
            View.CharacterHUD.gameObject.SetActive(true);
            // 체력, 액션포인트, 초상화 설정
            View.CharacterHUD.ShowCharacterHUD(curHp, maxHp, curAp, maxAp);
            // 스킬 버튼 생성
            View.CharacterHUD.SkillPanel.SetSkillButtons(model.Skills);
            
        }
        #endregion

        #region View To Model

        public void OnClickTurnEndButton()
        {
            Debug.Log("턴 종료");
        }
        public void OnClickMenuButton()
        {
            Debug.Log("메뉴창 열기");
        }
        public void OnClickPortraitButton()
        {
            Debug.Log("캐릭터 정보창 띄우기");
        }

        #region ExtraActions
        public void OnClickJumpButton()
        {
            Debug.Log("점프");
        }

        public void OnClickPushButton()
        {
            Debug.Log("밀기");
        }
        public void OnClickInvenButton()
        {
            Debug.Log("인벤토리 오픈");
        }
        #endregion
        
        #endregion

    }
}



