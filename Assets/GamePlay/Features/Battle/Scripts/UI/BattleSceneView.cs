using AngelBeat;
using Core.Scripts.Foundation.Define;
using Core.Scripts.Managers;
using Cysharp.Threading.Tasks;
using GamePlay.Features.Battle.Scripts.BattleAction;
using GamePlay.Features.Battle.Scripts.BattleTurn;
using GamePlay.Features.Battle.Scripts.Models;
using GamePlay.Features.Battle.Scripts.UI.UIObjects;
using GamePlay.Features.Battle.Scripts.Unit;
using System;
using System.Collections.Generic;
using System.Threading;
using UIs.Runtime;
using UnityEngine;
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
        
        #region External UI Getter By string key
        public RectTransform GetTutorialTarget(string key)
        {
            switch (key)
            {
                case "JumpButton":
                    return characterHUD.JumpButton.transform as RectTransform;
                case "EndTurnButton":
                    return turnEndButton.transform as RectTransform;
                case "PushButton":
                    return characterHUD.PushButton.transform as RectTransform;
                default:
                    return null;
            }
        }
        #endregion
        
    }

    public class BattleHUDPresenter : PresenterBase<BattleHUDView>
    {
        #region AddressablesDefinition
        private readonly string _turnPortraitAddress = "TurnPortrait";
        #endregion
        
        
        
        
        public struct SkillResourceRoot
        {
            public string iconRoot;
            public string descriptionRoot;

            public SkillResourceRoot(string iconRoot, string descriptionRoot)
            {
                this.iconRoot = iconRoot;
                this.descriptionRoot = descriptionRoot;
            }
        }
        
        
        
        public BattleHUDPresenter(IView view) : base(view) { }

        public override UniTask EnterAction(CancellationToken token)
        {
            #region Model Events
            
            // 턴 바뀜 이벤트 구독
            ModelEvents.Subscribe<TurnChangedDTO>(
                act => BattleController.Instance.TurnController.OnTurnChanged += act,
                act => BattleController.Instance.TurnController.OnTurnChanged -= act,
                OnTurnChanged
            );
            
            ModelEvents.Subscribe<TurnActionDTO>(
                act => BattleController.Instance.TurnController.OnCurrentTurnActionChanged += act,
                act => BattleController.Instance.TurnController.OnCurrentTurnActionChanged -= act,
                OnCurrentTurnActionChanged
            );
            
            ModelEvents.Subscribe<TurnOrderDTO>(
                act => BattleController.Instance.TurnController.OnTurnOrderChanged += act,
                act => BattleController.Instance.TurnController.OnTurnOrderChanged -= act,
                OnRoundChanged
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

        public override UniTask ExitAction(CancellationToken token)
        {
            _focusCharacterEvents.Clear();
            return UniTask.CompletedTask;
        }


        #region Model To View

        private readonly PresenterEventBag _focusCharacterEvents = new();
        private bool _turnPortraitsReady;
        private bool _pendingFirstHighlight;

        private void OnRoundChanged(TurnOrderDTO dto)
        {
            View.TurnHUD.ClearList();
            View.TurnHUD.OnRoundStart();
            _turnPortraitsReady = false;
            _pendingFirstHighlight = false;
            InstantiateTurnPortrait(dto).Forget();
        }
        
        private async UniTask InstantiateTurnPortrait(TurnOrderDTO dto)
        {
            foreach (TurnSlotDTO turn in dto.Slots)
            {
                if (turn.IsDead) continue;
                CharBase character = BattleCharManager.Instance.GetFieldChar(turn.ActorId);
                
                UniTask<GameObject> task = ResourceManager.Instance.InstantiateAsync(_turnPortraitAddress, View.TurnHUD.transform);
                TurnPortrait turnPortrait = (await task).GetComponent<TurnPortrait>();
                
                string root = character.CharInfo.IconSpriteRoot;
                Sprite sprite = await ResourceManager.Instance.LoadAsync<Sprite>(root);
                Debug.Log(sprite);
                turnPortrait.SetPortraitImage(sprite, turn.ActorId);
                
                View.TurnHUD.AddToTurnList(turnPortrait);
            }
            
            _turnPortraitsReady = true;
            
            if (_pendingFirstHighlight)
            {
                _pendingFirstHighlight = false;
                View.TurnHUD.MoveToNextTurn();
            }
        }

        private void OnTurnChanged(TurnChangedDTO turnModel)
        {
            // 현재 턴 표시자 옮기기
            if (!_turnPortraitsReady)
            {
                _pendingFirstHighlight = true;
            }
            else
            {
                View.TurnHUD.MoveToNextTurn();
            }
            
            // 아군 턴이면 캐릭터 HUD 오픈
            CharBase character = BattleCharManager.Instance.GetFieldChar(turnModel.ActorId);
            if (!character)
            {
                Debug.Log("[Battle Turn Changed Event] : Invalid Character ");
                return;
            }
            
            if (turnModel.Side == SystemEnum.eCharType.Player)
            {
                OnCharacterHUDOpen(character);
            }
            else
            {
                _focusCharacterEvents.Clear();
                View.CharacterHUD.Hide();
            }
        }

        private async void OnCharacterHUDOpen(CharBase character)
        {
            _focusCharacterEvents.Clear();
            // 초상화
            Sprite charPortrait = await ResourceManager.Instance.LoadAsync<Sprite>($"BattlePanel_{character.CharInfo.IconSpriteRoot}");
            // 이름
            string name = character.CharInfo.Name;
            Debug.Log("캐릭터 이름 : " + name);
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
            
            Sprite characterPortrait = await ResourceManager.Instance.LoadAsync<Sprite>($"BattlePanel_{character.CharInfo.IconSpriteRoot}");
            // HUD 패널 오픈
            View.CharacterHUD.gameObject.SetActive(true);
            // 체력, 액션포인트, 초상화 설정
            View.CharacterHUD.ShowCharacterHUD(name, curHp, maxHp, curAp, maxAp, characterPortrait);



            List<SkillResourceRoot> skillRoots = new();
            
            foreach (var skill in character.SkillInfo.SkillSlots)
            {
                Debug.Log(skill.SkillName);
                
                SkillResourceRoot roots = new SkillResourceRoot(skill.Icon, skill.TooltipName);
                skillRoots.Add(roots);
            }
            // 스킬 버튼 생성
            View.CharacterHUD.SetSkillButtons(skillRoots);

            
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
        
        
        private void OnCurrentTurnActionChanged(TurnActionDTO dto)
        {
            bool isPlayer = BattleCharManager.Instance
                .GetFieldChar(dto.ActorId)?.GetCharType() == SystemEnum.eCharType.Player;

            if (!isPlayer) return;
            View.CharacterHUD.DisableAllToggleButton();
            View.CharacterHUD.SetSkillInteractable(dto.CanUseSkill);
            View.CharacterHUD.SetExtraInteractable(dto.CanUseExtra);
        }
        
        #endregion

        #region View To Model

        bool _turnEnding;
        private async void OnClickTurnEndButton()
        {
            if (BattleController.Instance.IsModal) return;
            
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
            _ = UIManager.Instance.ShowViewAsync(ViewID.CharacterInfoPopUpView);
        }

        private void OnSkillSelected(int slot)
        {
            BattleInputGate gate = BattleInputGate.Instance;
            if (gate && !gate.CanStartAction(ActionType.Skill))
            {
                SkillButton btn = View.CharacterHUD.SkillPanel.SkillButtons[slot].GetComponent<SkillButton>();
                View.CharacterHUD.Toggling(btn);

                // TODO : SystemMessage 띄울 것
                Debug.Log("[Tutorial] 이 스킬은 지금 사용할 수 없습니다.");

                return;
            }
            
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
            BattleInputGate gate = BattleInputGate.Instance;
            if (gate && !gate.CanStartAction(ActionType.Jump))
            {
                View.CharacterHUD.Toggling(View.CharacterHUD.JumpButton);
                Debug.Log("[InputGate] 지금은 점프를 사용할 수 없습니다.");
                return;
            }
            
            if (View.CharacterHUD.JumpButton.GetComponent<ToggleButton>().isSelected)
            {
                Debug.Log("점프");
                BattleController.Instance.StartPreview(ActionType.Jump).Forget();
            }
        }

        private void OnClickPushButton()
        {
            BattleInputGate gate = BattleInputGate.Instance;
            if (gate && !gate.CanStartAction(ActionType.Push))
            {
                View.CharacterHUD.Toggling(View.CharacterHUD.PushButton);
                Debug.Log("[InputGate] 지금은 밀기를 사용할 수 없습니다.");
                return;
            }
            
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



