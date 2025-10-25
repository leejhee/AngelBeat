using Core.Scripts.Foundation.Define;
using Core.Scripts.Managers;
using Cysharp.Threading.Tasks;
using GamePlay.Common.Scripts.Entities.Character;
using GamePlay.Features.Explore.Scripts.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UIs.Runtime;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace GamePlay.Features.Explore.Scripts.UI
{
    public class ExploreHUDView : MonoBehaviour, IView
    {
        public GameObject Root { get; }

        [SerializeField] private Transform partyTransform;
        [SerializeField] private Image lunarPhase;
        [SerializeField] private List<ExploreResource> exploreResources;
        
        public Transform PartyTransform => partyTransform;
        public List<ExploreResource> ExploreResources => exploreResources;
        
        
        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);

        public UniTask PlayEnterAsync(CancellationToken ct) => UniTask.CompletedTask;
        public UniTask PlayExitAsync(CancellationToken ct) => UniTask.CompletedTask;
    }

    public class ExploreHUDPresenter : PresenterBase<ExploreHUDView>
    {
        public ExploreHUDPresenter(IView view) : base(view)
        {
        }
        private const string PartyCharacterAddress = "PartyCharacter";
        
        private Dictionary<long ,ExplorePartyCharacterUI> _exploreCharacters;

        public override UniTask EnterAction(CancellationToken token)
        {
            // 재화 수치 변경 이벤트 구독
            
            
            
            // 파티원 초상화 생성
            
            
            
            return UniTask.CompletedTask;
        }
            
        
        // 보유 재화 수치 변경
        public void ChangeResourceAmount(ExploreResourceModel model)
        {
            foreach (ExploreResource resource in View.ExploreResources.Where(resource => resource.ResourceType == model.ResourceType))
            {
                resource.SetResourceAmount(model.Amount);
                break;
            }
        }

        // public void InstantiateAllParty()
        // {
        //     // 현재 파티원 전원 만들어주기
        //     foreach (var character in party)
        //     {
        //         
        //     }
        // }
        
        public async void InstantiatePartyPortrait(CharacterModel model)
        {
            UniTask<GameObject> goTask =  ResourceManager.Instance.InstantiateAsync(PartyCharacterAddress, View.PartyTransform);
            ExplorePartyCharacterUI exploreCharacter = (await goTask).GetComponent<ExplorePartyCharacterUI>();

            UniTask<Sprite> spriteTask = ResourceManager.Instance.LoadAsync<Sprite>(model.LdRoot);
            Sprite sprite = await spriteTask;
            
            exploreCharacter.InitExplorePartyCharacter(
                sprite, 
                model.Name,
                model.BaseStat.GetStat(SystemEnum.eStats.NHP),
                model.BaseStat.GetStat(SystemEnum.eStats.NMHP)
                );
            
            _exploreCharacters.Add(model.Index, exploreCharacter);
        }

    }
}
