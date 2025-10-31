using Core.Scripts.Foundation.Define;
using Core.Scripts.Foundation.SceneUtil;
using Cysharp.Threading.Tasks;
using GamePlay.Features.Battle.Scripts;
using GamePlay.Features.Battle.Scripts.UI.UIObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UIs.Runtime;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace AngelBeat.UI
{
    // 보상을 위한 임시 구조체 > 이거 다른 보상들이랑 공유하는 추상 클래스의 자식으로 만들 예정
    [Serializable]
    public struct SKillReward
    {
        public long id;
        public Sprite selected;
        public Sprite deSelected;

        public SKillReward(long id, Sprite selected = null, Sprite deSelected = null)
        {
            this.id = id;
            this.selected = selected;
            this.deSelected = deSelected;
        }
    }
    
    public class BattleWinView : MonoBehaviour, IView
    {

        // 얘도 나중에 바꿔줄거임
        [Serializable]
        public struct RewardTable
        {
            public string name;
            public List<SKillReward> rewardRange;
        }
        
        [SerializeField] private GameObject rewardUIPrefab;
        [SerializeField] private List<RewardTable> rewardList;
        [SerializeField] private Transform rewardPanel;

        [SerializeField] private Button getRewardButton;
        
        [SerializeField] private List<ToggleButton> buttons = new List<ToggleButton>();

        public void AddToggleButton(ToggleButton toggleButton)
        {
            if (toggleButton is RewardObject reward)
            {
                reward.InterActionButton.onClick.AddListener(()=> Toggling(reward));
            }
            buttons.Add(toggleButton);
        }
        public void Toggling(ToggleButton selectedButton)
        {
            // 이미 선택된 버튼을 클릭시
            if (selectedButton.isSelected)
            {
                selectedButton.isSelected = false;
                selectedButton.Frame.sprite = selectedButton.NonSelectedFrame;
                selectedButton.OnDeselect();
            }
            else
            {
                foreach (var button in buttons)
                {
                    if (button.isSelected)
                    {
                        button.isSelected = false;
                        button.Frame.sprite = button.NonSelectedFrame;
                        button.OnDeselect();
                    }

                }
                selectedButton.isSelected = true;
                selectedButton.Frame.sprite = selectedButton.SelectedFrame;
                selectedButton.OnSelect();
            }
        }
        
        
        public Button GetRewardButton => getRewardButton;
        public List<RewardTable>  RewardList => rewardList;
        public GameObject RewardUIPrefab => rewardUIPrefab;
        public Transform RewardPanel => rewardPanel;


        public GameObject Root { get; }
        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);

        public UniTask PlayEnterAsync(CancellationToken ct) => UniTask.CompletedTask;
        public UniTask PlayExitAsync(CancellationToken ct) => UniTask.CompletedTask;
    }
    public class BattleWinPresenter : PresenterBase<BattleWinView>
    {
        private long _selectedSkillId;
        private List<long> _rewardList = new();
        
        public BattleWinPresenter(IView view) : base(view)
        {
            
        }

        public override UniTask EnterAction(CancellationToken token)
        {
            InstantiateSkillRewardObjects();
            
            
            // ModelEvents.Subscribe<long>(
            //     action => BattleController.Instance.rewardSkillSelectedEvent += action,
            //     action => BattleController.Instance.rewardSkillSelectedEvent -= action,
            //     SelectRewardSkill
            //         );
            
            ViewEvents.Subscribe(
                act => View.GetRewardButton.onClick.AddListener(new UnityAction(act)),
                act => View.GetRewardButton.onClick.RemoveAllListeners(),
                GetReward
            );
            
            return UniTask.CompletedTask;
        }

        private readonly PresenterEventBag _eventBag = new();
        
        private void InstantiateSkillRewardObjects()
        {
            // 이거 나중에 보상 테이블에서 가져오도록 설정할것 일단 뷰에서 가져오는거로 함
            
            // 리워드 테이블에서 Skill 태그 있는것
            foreach (BattleWinView.RewardTable table in View.RewardList.Where(table => table.name == "Skill"))
            {
                
                // 보상 3개 뽑기
                for (int i = 0; i < 3; i++)
                {
                    // 테이블에 있는 스킬 개수에서 랜덤으로 정수 뽑음
                    int randIndex =  UnityEngine.Random.Range(0, table.rewardRange.Count);
                    SKillReward reward = table.rewardRange[randIndex];
                    
                    long skillId = table.rewardRange[randIndex].id;
                    
                    //랜덤 정수 번째 스킬
                    if (_rewardList.Contains(skillId))
                    {
                        // 이미 딕셔너리에 스킬이 포함되어 있으면 다시한번 추첨
                        i--;
                    }
                    else
                    {
                        _rewardList.Add(skillId);
                        
                        GameObject rewardObject = Object.Instantiate(View.RewardUIPrefab, View.RewardPanel, true);
                        RewardObject rewardObj = rewardObject.GetComponent<RewardObject>();
                        
                        // 보상 인덱스 및 스프라이트 설정
                        rewardObj.SetReward(i, reward.deSelected, reward.selected);

                        // 토글 리스트에 추가
                        View.AddToggleButton(rewardObj);
                        
                        // 단순한 index만 전달하자.
                        _eventBag.Subscribe<int>(
                            act =>
                            {
                                rewardObj.Selected -= act;
                                rewardObj.Selected += act;
                            },
                            act=> rewardObj.Selected -= act,
                            SelectRewardSkill
                        );
                    }
                }
            }
        }
        private void SelectRewardSkill(int idx)
        {
            _selectedSkillId = _rewardList[idx];
        }
        private void GetReward()
        {
            //selectedSkillId << 이 아이디의 스킬을 넣어라
            Debug.Log(_selectedSkillId);
            BattleController.Instance.GetSkill(_selectedSkillId);
            
            // 이후 다음 전투
            
            //SceneLoader.LoadSceneWithLoading(SystemEnum.eScene.LobbyScene);
        }
    }
}