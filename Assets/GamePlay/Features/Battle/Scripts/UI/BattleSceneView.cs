using Core.Scripts.Managers;
using Cysharp.Threading.Tasks;
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
        
        public Button test => turnEndButton;
        
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
            //hp 구독
            ModelEvents.Subscribe<HPModel>(
                act => BattleController.Instance.FocusChar.CharStat.OnHPChanged += act,
                act => BattleController.Instance.FocusChar.CharStat.OnHPChanged -= act,
                OnHPChanged);

            ViewEvents.Subscribe(
                act => View.test.onClick.AddListener(new UnityAction(act)),
                act => View.test.onClick.RemoveAllListeners(),
                OnClickButton
                );
            
            return UniTask.CompletedTask;
        }

        private void OnHPChanged(HPModel model)
        {
            int delta = model.Delta;
            View.ChangeHP(delta);
        }
        public void OnClickButton()
        {
            BattleController.Instance.FocusChar.CharStat.ChangeHP(5);
        }
    }

    public class HPModel
    {
        public int Delta;

        public HPModel(int delta) => Delta = delta;
    }
}



