using Core.Scripts.Foundation.Define;
using Core.Scripts.Managers;
using Cysharp.Threading.Tasks;
using GamePlay.Common.Scripts.Entities.Character;
using GamePlay.Common.Scripts.Entities.Skills;
using GamePlay.Features.Battle.Scripts.BattleTurn;
using GamePlay.Features.Battle.Scripts.Models;
using GamePlay.Features.Battle.Scripts.UI.UIObjects;
using GamePlay.Features.Battle.Scripts.Unit;
using GamePlay.Skill;
using System.Linq;
using System.Threading;
using UIs.Runtime;
using Unity.VisualScripting;
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
        [SerializeField] private TurnHUD turnHUD;

        public Button TurnEndButton => turnEndButton;
        public Button MenuButton => menuButton;
        public CharacterHUD CharacterHUD => characterHUD;
        public TurnHUD TurnHUD => turnHUD;
        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);

        public UniTask PlayEnterAsync(CancellationToken ct) => UniTask.CompletedTask;
        public UniTask PlayExitAsync(CancellationToken ct) => UniTask.CompletedTask;
        

        public void ChangeHp(long delta)
        {
            characterHUD.ChangeHpUI(delta);
        }

        public void ChangeAp(long delta)
        {
            characterHUD.ChangeApUI(delta);
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
                act => BattleController.Instance.FocusChar.CharStat.OnFocusedCharHpChanged += act,
                act => BattleController.Instance.FocusChar.CharStat.OnFocusedCharHpChanged -= act,
                OnHPChanged);
            ModelEvents.Subscribe<ApModel>(
                action =>   BattleController.Instance.FocusChar.CharStat.OnFocusedCharApChanged += action,
                action => BattleController.Instance.FocusChar.CharStat.OnFocusedCharApChanged -= action,
                OnAPChanged
                );
            // 턴 바뀜 이벤트 구독
            ModelEvents.Subscribe<TurnController.TurnModel>(
                act => BattleController.Instance.TurnController.OnTurnChanged += act,
                act => BattleController.Instance.TurnController.OnTurnChanged -= act,
                OnTurnChanged
                    );

            ModelEvents.SubscribeAsync(
                act => BattleController.Instance.TurnController.OnRoundProceeds += act,
                act => BattleController.Instance.TurnController.OnRoundProceeds -= act,
                InstantiateTurnPortrait
                );

            ModelEvents.Subscribe(
                act => BattleController.Instance.TurnController.OnRoundEnd += act,
                act => BattleController.Instance.TurnController.OnRoundEnd -= act,
                OnRoundEnd
                );
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
            ViewEvents.Subscribe(
                act => View.CharacterHUD.CharacterPortrait.CharacterInfoPopup += act,
                act => View.CharacterHUD.CharacterPortrait.CharacterInfoPopup -= act,
                OnClickPortraitButton
            );

            ViewEvents.Subscribe(
                act => View.CharacterHUD.JumpButton.ActionButton.onClick.AddListener(new UnityAction(act)),
                act => View.CharacterHUD.JumpButton.ActionButton.onClick.RemoveAllListeners(),
                OnClickJumpButton
            );
            ViewEvents.Subscribe(
                act => View.CharacterHUD.PushButton.ActionButton.onClick.AddListener(new UnityAction(act)),
                act => View.CharacterHUD.PushButton.ActionButton.onClick.RemoveAllListeners(),
                OnClickPushButton
                );
            ViewEvents.Subscribe(
                act => View.CharacterHUD.InvenButton.ActionButton.onClick.AddListener(new UnityAction(act)),
                act => View.CharacterHUD.InvenButton.ActionButton.onClick.RemoveAllListeners(),
                OnClickInvenButton
                );

            // for (int i = 0; i < View.CharacterHUD.SkillPanel.SkillButtons.Count; i++)
            // {
            //     int temp = i;
            //     var skill =  View.CharacterHUD.SkillPanel.SkillButtons[temp];
            //     Button skillButton = skill.GetComponent<Button>();
            //     UnityAction wrapper = null;
            //     ViewEvents.Subscribe<int>(
            //         add: h => { wrapper = () => h(temp); skillButton.onClick.AddListener(wrapper); },
            //         remove: h => { if (wrapper != null) skillButton.onClick.RemoveListener(wrapper); },
            //         handler: skill.OnClickSkillButton 
            //     );
            // }
            
            //foreach (var skill in View.CharacterHUD.SkillPanel.SkillButtons)
            //{
            //    Button skillButton = skill.GetComponent<Button>();
            //    ViewEvents.Subscribe(
            //        act => skillButton.onClick.AddListener(new UnityAction(act)),
            //        act => skillButton.onClick.RemoveAllListeners(),
            //        skill.OnClickSkillButton
            //    );
            //}

            #endregion
            return UniTask.CompletedTask;
        }


        #region Model To View
        private void OnHPChanged(HPModel model)
        {
            long delta = model.Delta;
            View.ChangeHp(delta);
        }

        private void OnAPChanged(ApModel model)
        {
            int delta = model.Delta;
            View.ChangeAp(delta);
        }

        private void OnRoundStart()
        {
            // 턴 HUD 초상화 생성
            GameObject InstantiatePortrait(CharBase charBase)
            {
                // TODO: 나중에 바꿀것
                GameObject go = ResourceManager.Instance.InstantiateAsync(_turnPortraitAddress, View.TurnHUD.gameObject.transform).GetAwaiter().GetResult();
                // 캐릭터 초상화 설정
                //go.GetComponent<TurnPortrait>().SetPortraitImage(charBase., charBase.GetID());
                return go;
            }

            foreach (Turn turn in BattleController.Instance.TurnController.TurnCollection)
            {
                GameObject turnObject = InstantiatePortrait(turn.TurnOwner);
                // 턴 초상화 오브젝트 만들어서 리스트에 넣기
                View.TurnHUD.AddToTurnList(turnObject.GetComponent<TurnPortrait>());
            }
            View.TurnHUD.OnRoundStart(); 
        }

        private async UniTask InstantiateTurnPortrait()
        {
            foreach (Turn turn in BattleController.Instance.TurnController.TurnCollection)
            {
                UniTask<GameObject> task = ResourceManager.Instance.InstantiateAsync(_turnPortraitAddress, View.TurnHUD.transform);
                TurnPortrait turnPortrait = (await task).GetComponent<TurnPortrait>();
                string root = turn.TurnOwner.CharInfo.IconSpriteRoot;
                Sprite sprite = DataManager.Instance.CharacterIconSpriteMap[root];
                Debug.Log(sprite);
                turnPortrait.SetPortraitImage(sprite, turn.TurnOwner.GetID());
                
                View.TurnHUD.AddToTurnList(turnPortrait);
            }
            View.TurnHUD.OnRoundStart();

            OnTurnChanged(new TurnController.TurnModel(BattleController.Instance.TurnController.TurnCollection.First()));

        }
        
        private void OnRoundEnd()
        {
            View.TurnHUD.ClearList();
        }

        private void OnTurnChanged(TurnController.TurnModel turnModel)
        {
            // 현재 턴 표시자 옮기기
            View.TurnHUD.MoveToNextTurn();
            // 아군 턴이면 캐릭터 HDU 오픈
            CharacterModel charModel = turnModel.Turn.TurnOwner.CharInfo;
             if (charModel.CharacterType == SystemEnum.eCharType.Player)
             {
                 OnCharacterHUDOpen(charModel);
             }
             else
             {
                 View.CharacterHUD.Hide();
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
            
            // 스킬 버튼마다 이벤트 구독
            for (int i = 0; i < model.Skills.Count; i++)
            {
                int idx = i;
                Button curSkillButton = View.CharacterHUD.SkillPanel.SkillButtons[idx].GetComponent<Button>();
                curSkillButton.onClick.RemoveAllListeners();
                
                
                ViewEvents.Subscribe<SkillModel>(
                    act => curSkillButton.onClick.AddListener(new UnityAction(() => act(model.Skills[idx]))),
                    act=> curSkillButton.onClick.RemoveAllListeners(),
                    skill => BattleController.Instance.TogglePreview(skill)
                );
            }
        }
        #endregion

        #region View To Model

        private void OnClickTurnEndButton()
        {
            BattleController.Instance.TurnController.ChangeTurn();
        }

        private void OnClickMenuButton()
        {
            Debug.Log("메뉴창 열기");
        }

        private void OnClickPortraitButton()
        {
            // 캐릭터 정보 팝업 뷰 생성
            _ = UIManager.Instance.ShowViewAsync(ViewID.BattleCharacterInfoPopUpView);
        }

        #region ExtraActions

        private void OnClickJumpButton()
        {
            if (View.CharacterHUD.JumpButton.GetComponent<ToggleButton>().isSelected)
            {
                Debug.Log("점프");
            }
        }

        private void OnClickPushButton()
        {
            if (View.CharacterHUD.PushButton.GetComponent<ToggleButton>().isSelected)
            {
                Debug.Log("밀기");
                BattleController.Instance.ShowPushPreview();
            }
        }

        private void OnClickInvenButton()
        {
            if (View.CharacterHUD.InvenButton.GetComponent<ToggleButton>().isSelected)
            {
                Debug.Log("인벤토리 오픈");
            }
        }
        #endregion
        
        #endregion

    }
}



