using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UIs.Runtime;
using UIs.UIObjects;
using UnityEngine;

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
        void Start()
        {
            List<rewardTable> list = new() { rewardList[0] };
            for (int i = 1; i < rewardList.Count; i++)
            {
                if (UnityEngine.Random.Range(0, 100) < 50)
                    list.Add(rewardList[i]);
            }
            foreach (var reward in list)
            {
                RewardObject obj = Instantiate(rewardUIPrefab.GetComponent<RewardObject>(), rewardPanel);
                rewardObjects.Add(obj);
                obj.OnClickReward += () =>
                {
                    if (rewardObjects.Count == 0)
                        StartCoroutine(QuitCountDown());
                };
                obj.SetReward(reward.name, reward.rewardRange[UnityEngine.Random.Range(0, reward.rewardRange.Count)]);
                
            }
        }

        IEnumerator QuitCountDown()
        {
            yield return new WaitForSeconds(3);
            Application.Quit();
        }

        public GameObject Root { get; }
        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);

        public UniTask PlayEnterAsync(CancellationToken ct) => UniTask.CompletedTask;
        public UniTask PlayExitAsync(CancellationToken ct) => UniTask.CompletedTask;
    }
    public class BattleWinPresenter : PresenterBase<BattleWinView>
    {
        public BattleWinPresenter(IView view) : base(view)
        {
            
        }

        public override UniTask EnterAction(CancellationToken token)
        {
            
            
            return UniTask.CompletedTask;
        }
    }
}