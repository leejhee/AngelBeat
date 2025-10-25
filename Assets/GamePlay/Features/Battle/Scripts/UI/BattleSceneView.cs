using AngelBeat;
using Core.Scripts.Foundation.Define;
using Core.Scripts.Managers;
using Cysharp.Threading.Tasks;
using GamePlay.Common.Scripts.Entities.Character;
using GamePlay.Features.Battle.Scripts.BattleAction;
using GamePlay.Features.Battle.Scripts.BattleTurn;
using GamePlay.Features.Battle.Scripts.Models;
using GamePlay.Features.Battle.Scripts.UI.UIObjects;
using GamePlay.Features.Battle.Scripts.Unit;
using System;
using System.Linq;
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
            
            //hp 구독 -> OnTurnChanged로 옮김. FocusChar을 참조하지 말 것
            //ModelEvents.Subscribe<HPModel>(
            //    act => BattleController.Instance.FocusChar.RuntimeStat.OnFocusedCharHpChanged += act,
            //    act => BattleController.Instance.FocusChar.RuntimeStat.OnFocusedCharHpChanged -= act,
            //    OnHPChanged);
            //ModelEvents.Subscribe<ApModel>(
            //    action =>   BattleController.Instance.FocusChar.RuntimeStat.OnFocusedCharApChanged += action,
            //    action => BattleController.Instance.FocusChar.RuntimeStat.OnFocusedCharApChanged -= action,
            //    OnAPChanged
            //    );
            
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
            
            // 행동 종료 후 토글 해제
            ModelEvents.Subscribe<BattleActionBase, BattleActionResult>(
                act => BattleController.Instance.ActionCompleted += act,
                act => BattleController.Instance.ActionCompleted -= act,
                (a, r) => { View.CharacterHUD.DeselectAllToggleButton(); }
            );

            ModelEvents.Subscribe<long>(
                act => BattleController.Instance.OnCharacterDead += act,
                act => BattleController.Instance.OnCharacterDead -= act,
                uid => View.TurnHUD.FindDeadCharacter(uid)
            );
            
            #endregion

            #region View Events
            
            // 턴 종료 버튼 구독
            ViewEvents.Subscribe(
                View.TurnEndButton.onClick,
                OnClickTurnEndButton
            );
            
            // 메뉴 버튼 구독
            ViewEvents.Subscribe(
                View.MenuButton.onClick,
                OnClickMenuButton
            );
            // 캐릭터 초상화 꾹 누르기
            ViewEvents.Subscribe(
                act => View.CharacterHUD.CharacterPortrait.CharacterInfoPopup += act,
                act => View.CharacterHUD.CharacterPortrait.CharacterInfoPopup -= act,
                OnClickPortraitButton
            );
            
            // 점프 - selected 구독
            ViewEvents.Subscribe(
                act =>
                {
                    View.CharacterHUD.JumpButton.Selected -= act;
                    View.CharacterHUD.JumpButton.Selected += act;
                },
                act => View.CharacterHUD.JumpButton.Selected -= act,
                OnClickJumpButton
            );
            
            // 점프 - unselected 구독
            ViewEvents.Subscribe(
                act =>
                {
                    View.CharacterHUD.JumpButton.UnSelected -= act;
                    View.CharacterHUD.JumpButton.UnSelected += act;
                },
                act => View.CharacterHUD.JumpButton.UnSelected -= act,
                OnActionDeselected
            );
            
            // 밀기 - selected 구독
            ViewEvents.Subscribe(
                act =>
                {
                    View.CharacterHUD.PushButton.Selected -= act;
                    View.CharacterHUD.PushButton.Selected += act;
                },
                act => View.CharacterHUD.PushButton.Selected -= act,
                OnClickPushButton
            );
            
            // 밀기 - unselected 구독
            ViewEvents.Subscribe(
                act =>
                {
                    View.CharacterHUD.PushButton.UnSelected -= act;
                    View.CharacterHUD.PushButton.UnSelected += act;
                },
                act => View.CharacterHUD.PushButton.UnSelected -= act,
                OnActionDeselected
            );
            
            // TODO : 인벤 구현 후 인벤 flow 작성할 것
            
            #endregion
            return UniTask.CompletedTask;
        }


        #region Model To View

        private readonly PresenterEventBag _focusCharacterEvents = new();
        
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
                Sprite sprite = await ResourceManager.Instance.LoadAsync<Sprite>(root);
                Debug.Log(sprite);
                turnPortrait.SetPortraitImage(sprite, turn.TurnOwner.GetID());
                
                View.TurnHUD.AddToTurnList(turnPortrait);
            }
            View.TurnHUD.OnRoundStart();
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
            //CharacterModel charModel = turnModel.Turn.TurnOwner.CharInfo;
            CharBase character = turnModel.Turn.TurnOwner;
            if (character.GetCharType() == SystemEnum.eCharType.Player)
            {
                OnCharacterHUDOpen(character);
            }
            else
            {
                _focusCharacterEvents.Clear();
                View.CharacterHUD.Hide();
            }
        }

        private void OnCharacterHUDOpen(CharBase character)
        {
            _focusCharacterEvents.Clear();
            // 초상화
            //Sprite charPortrait = model.
            // 이름
            string name = character.CharInfo.Name;
            // 현재 체력
            long curHp = character.RuntimeStat.GetStat(SystemEnum.eStats.NHP);
            long maxHp = character.RuntimeStat.GetStat(SystemEnum.eStats.NMHP);
            
            
            // 현재 액션포인트
            long curAp = character.RuntimeStat.GetStat(SystemEnum.eStats.NACTION_POINT);
            long maxAp = character.RuntimeStat.GetStat(SystemEnum.eStats.NMACTION_POINT);
            
            // 
            _focusCharacterEvents.Subscribe<SystemEnum.eStats, long, long>(
                act => character.RuntimeStat.OnStatChanged += act,
                act => character.RuntimeStat.OnStatChanged -= act,
                OnFocusStatChanged
            );
            
            // HUD 패널 오픈
            View.CharacterHUD.gameObject.SetActive(true);
            // 체력, 액션포인트, 초상화 설정
            View.CharacterHUD.ShowCharacterHUD(name, curHp, maxHp, curAp, maxAp);
            // 스킬 버튼 생성
            View.CharacterHUD.SetSkillButtons(character.SkillInfo.SkillSlots);

            
            // 스킬 버튼마다 이벤트 구독
            for (int i = 0; i < character.SkillInfo.SkillSlots.Count; i++)
            {
                int idx = i;
                SkillButton curSkillButton = View.CharacterHUD.SkillPanel.SkillButtons[idx].GetComponent<SkillButton>();
                
                // 단순한 index만 전달하자.
                _focusCharacterEvents.Subscribe<int>(
                    act =>
                    {
                        curSkillButton.Selected -= act;
                        curSkillButton.Selected += act;
                    },
                    act=> curSkillButton.Selected -= act,
                    OnSkillSelected
                );
                
                _focusCharacterEvents.Subscribe<int>(
                    act =>
                    {
                        curSkillButton.Deselected -= act;
                        curSkillButton.Deselected += act;
                    },
                    act=> curSkillButton.Deselected -= act,
                    OnSkillDeselected
                );
            }
        }

        private void OnFocusStatChanged(SystemEnum.eStats stat, long delta, long result)
        {
            if (delta == 0) return;
            if (stat == SystemEnum.eStats.NHP) View.ChangeHp(delta);
            else if (stat == SystemEnum.eStats.NACTION_POINT) View.ChangeAp(delta);
        }
        
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
        
        #endregion

        #region View To Model

        bool _turnEnding;
        private async void OnClickTurnEndButton()
        {
            if (_turnEnding) return;
            _turnEnding = true;
            View.TurnEndButton.interactable = false;

            var ct = View.gameObject.GetCancellationTokenOnDestroy();
            try
            {
                await BattleController.Instance.TurnController
                    .ChangeTurn()
                    .AttachExternalCancellation(ct);
            }
            catch (OperationCanceledException) { /* noop */ }
            catch (Exception ex) { Debug.LogException(ex); }
            finally
            {
                View.TurnEndButton.interactable = true;
                _turnEnding = false;
            }
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

        private void OnSkillSelected(int slot)
        {
            BattleController.Instance.StartPreview(ActionType.Skill, slot).Forget();
        }

        private void OnSkillDeselected(int slot) => OnActionDeselected();

        private void OnActionDeselected()
        {
            BattleController.Instance.CancelPreview();
        }
        
        #region ExtraActions

        private void OnClickJumpButton()
        {
            if (View.CharacterHUD.JumpButton.GetComponent<ToggleButton>().isSelected)
            {
                Debug.Log("점프");
                BattleController.Instance.StartPreview(ActionType.Jump).Forget();
            }
        }

        private void OnClickPushButton()
        {
            if (View.CharacterHUD.PushButton.GetComponent<ToggleButton>().isSelected)
            {
                Debug.Log("밀기");
                BattleController.Instance.StartPreview(ActionType.Push).Forget();
            }
        }

        private void OnClickInvenButton()
        {
            if (View.CharacterHUD.InvenButton.GetComponent<ToggleButton>().isSelected)
            {
                Debug.Log("인벤토리 오픈");
                //인벤토리 UI 오픈합시다
            }
        }
        #endregion
        
        #endregion

    }
}



