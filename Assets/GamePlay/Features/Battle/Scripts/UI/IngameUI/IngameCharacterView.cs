using Core.Scripts.Foundation.Define;
using Cysharp.Threading.Tasks;
using GamePlay.Common.Scripts.Entities.Character;
using GamePlay.Features.Battle.Scripts.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using UIs.Runtime;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using ResourceManager = Core.Scripts.Managers.ResourceManager;

namespace GamePlay.Features.Battle.Scripts.UI.IngameUI
{
    public class IngameCharacterView : MonoBehaviour, IView
    {
        public GameObject Root { get; }

        [SerializeField] private Transform keywordPanel;
        [SerializeField] private List<FloatingKeyword> _keywords = new List<FloatingKeyword>();
        [SerializeField] private Image hpFill;
        [SerializeField] private Button keywordButton;
        public Transform KeywordPanelTransform => keywordPanel;
        public List<FloatingKeyword> Keywords => _keywords;
        //public Image HPFill => hpFill;
        public Button KeywordButton => keywordButton;
        
        
        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);

        public UniTask PlayEnterAsync(CancellationToken ct) => UniTask.CompletedTask;
        public UniTask PlayExitAsync(CancellationToken ct) => UniTask.CompletedTask;


        public event Func<FloatingKeywordModel, UniTask> OnKeywordGet;

        public void TestButton()
        {
            OnKeywordGet.Invoke(new FloatingKeywordModel(3, 3));
        }


        public void InitHpFill()
        {
            hpFill.fillAmount = 1f;
        }
        
        public async void SetHpFill(long curHp, long maxHp)
        {
            await HpFillChanged(curHp, maxHp, hpFill);
        }

        private async UniTask HpFillChanged(long curHp, long maxHp, Image fill)
        {
            const float timeToEnd = 0.5f;
            float counter = 0f;
            float startValue = fill.fillAmount;
            float targetValue = (float)curHp/(float)maxHp;
            while (counter < timeToEnd)
            {
                counter += Time.deltaTime;
                float t = counter/timeToEnd;
                float curValue = Mathf.Lerp(startValue, targetValue, t);
                fill.fillAmount = curValue;
                await UniTask.Yield();
            }
            fill.fillAmount = targetValue;
        }
        
    }
    public class IngameCharacterPresenter : PresenterBase<IngameCharacterView>
    {
        private const string floatingKeywordAddress = "KeywordFloatingObject";
        
        public IngameCharacterPresenter(IView view) : base(view) { }
        public override UniTask EnterAction(CancellationToken token)
        {
            // 어떤 캐릭터이든 체력이 바꼈을때
            // ModelEvents.Subscribe<CharacterModel>(
            //     act => BattleController.Instance.,
            //     act => BattleController.Instance,
            //     OnHpChanged
            // );
            ModelEvents.SubscribeAsync<FloatingKeywordModel>(
                act => View.OnKeywordGet += act,
                act => View.OnKeywordGet -= act,
                InstantiateNewKeword
                );
            ViewEvents.Subscribe(
                act => View.KeywordButton.onClick.AddListener(new UnityAction(act)),
                act => View.KeywordButton.onClick.RemoveAllListeners(),
                OnClickKeyWordButton
            );
            
            return UniTask.CompletedTask;
        }
        private void OnHpChanged(CharacterModel charModel)
        {
            View.SetHpFill(charModel.BaseStat.GetStat(SystemEnum.eStats.NHP), charModel.BaseStat.GetStat(SystemEnum.eStats.NMHP));
        }

        private async UniTask InstantiateNewKeword(FloatingKeywordModel model)
        {
            UniTask<GameObject> task =
                ResourceManager.Instance.InstantiateAsync(floatingKeywordAddress, View.KeywordPanelTransform, true);

            FloatingKeyword keyword = (await task).GetComponent<FloatingKeyword>();
            keyword.OnInstantiated(model);
            View.Keywords.Add(keyword);
        }
        
        
        private void OnClickKeyWordButton()
        {
            UIManager.Instance.ShowViewAsync(ViewID.KeywordPopUpView);
        }
    }
} 
