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

        //private Dictionary<int, ExplorePartyCharacterUI> _exploreCharacters = new();

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
        
        public async void InstantiatePartyPortrait(List<CharacterModel> models)
        {
            for (int i = 0; i < models.Count; i++)
            {
                // UI 오브젝트 생성
                UniTask<GameObject> goTask =  ResourceManager.Instance.InstantiateAsync(PartyCharacterAddress, View.PartyTransform);
                ExplorePartyCharacterUI exploreCharacter = (await goTask).GetComponent<ExplorePartyCharacterUI>();
                
                // 스프라이트 불러오기
                UniTask<Sprite> spriteTask = ResourceManager.Instance.LoadAsync<Sprite>(models[i].LdRoot);
                Sprite sprite = await spriteTask;
                
                // 이미지, 체력 설정
                exploreCharacter.InitExplorePartyCharacter(
                    sprite,
                    models[i].BaseStat.GetStat(SystemEnum.eStats.NHP),
                    models[i].BaseStat.GetStat(SystemEnum.eStats.NMHP),
                    i);
                
                
                // 딕셔너리에 넣어주기 이거 필요한지 고민
                //_exploreCharacters.Add(i, exploreCharacter);
            }
        }

    }
}
