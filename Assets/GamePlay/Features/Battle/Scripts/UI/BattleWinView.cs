using Core.Scripts.Foundation.Define;
using Core.Scripts.Foundation.SceneUtil;
using Cysharp.Threading.Tasks;
using GamePlay.Features.Battle.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UIs.Runtime;
using UIs.UIObjects;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AngelBeat.UI
{
    public class BattleWinView : MonoBehaviour, IView
    {
        [Serializable]
        public struct rewardTable
        {
            public string name;
            public List<long> rewardRange;
        }
        
        [SerializeField] private GameObject rewardUIPrefab;
        [SerializeField] private List<rewardTable> rewardList;
        [SerializeField] private Transform rewardPanel;
        
        private List<RewardObject> rewardObjects = new();

        [SerializeField] private Button getRewardButton;
        public Button GetRewardButton => getRewardButton;
        // void Start()
        // {
        //     List<rewardTable> list = new() { rewardList[0] };
        //     for (int i = 1; i < rewardList.Count; i++)
        //     {
        //         if (UnityEngine.Random.Range(0, 100) < 50)
        //             list.Add(rewardList[i]);
        //     }
        //     foreach (var reward in list)
        //     {
        //         RewardObject obj = Instantiate(rewardUIPrefab.GetComponent<RewardObject>(), rewardPanel);
        //         rewardObjects.Add(obj);
        //         obj.OnClickReward += () =>
        //         {
        //             if (rewardObjects.Count == 0)
        //                 StartCoroutine(QuitCountDown());
        //         };
        //         obj.SetReward(reward.name, reward.rewardRange[UnityEngine.Random.Range(0, reward.rewardRange.Count)]);
        //         
        //     }
        // }


        public GameObject Root { get; }
        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);

        public UniTask PlayEnterAsync(CancellationToken ct) => UniTask.CompletedTask;
        public UniTask PlayExitAsync(CancellationToken ct) => UniTask.CompletedTask;
    }
    public class BattleWinPresenter : PresenterBase<BattleWinView>
    {
        private long selectedSkillId;
        
        public BattleWinPresenter(IView view) : base(view)
        {
            
        }

        public override UniTask EnterAction(CancellationToken token)
        {
            ModelEvents.Subscribe<long>(
                action => BattleController.Instance.rewardSkillSelectedEvent += action,
                action => BattleController.Instance.rewardSkillSelectedEvent -= action,
                SelectRewardSkill
                    );
            
            ViewEvents.Subscribe(
                act => View.GetRewardButton.onClick.AddListener(new UnityAction(act)),
                act => View.GetRewardButton.onClick.RemoveAllListeners(),
                GetReward
            );
            
            return UniTask.CompletedTask;
        }

        private readonly PresenterEventBag _eventBag = new();

        private void SelectRewardSkill(long skillID)
        {
            selectedSkillId =  skillID;
        }
        private void GetReward()
        {
            //selectedSkillId << 이 아이디의 스킬을 넣어라
            
            
            
            //SceneLoader.LoadSceneWithLoading(SystemEnum.eScene.LobbyScene);
        }
    }
}