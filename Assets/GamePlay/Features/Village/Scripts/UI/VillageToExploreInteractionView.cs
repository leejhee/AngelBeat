using Cysharp.Threading.Tasks;
using System.Threading;
using UIs.Runtime;
using UnityEngine;

namespace GamePlay.Features.Village.Scripts.UI
{
    // 가이드 관련 string도 데이터화하는게 맞나? 추후 결정 예정.
    // 데이터화되면, 이제 데이터로부터 symbol의 '이름'에 따라 가져올 예정.
    
    /// <summary>
    /// 저장 X. 탐사 내 상호작용 오브젝트일 경우, 범위 내에 접근 시 뜨는 팝업
    /// </summary>
    public class VillageToExploreInteractionView : MonoBehaviour, IView
    {
        public GameObject Root { get; }
        
        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);

        public UniTask PlayEnterAsync(CancellationToken ct) => UniTask.CompletedTask;
        public UniTask PlayExitAsync(CancellationToken ct) => UniTask.CompletedTask;

        public async UniTask ShowExploreGuideOnClick()
        {
            await UIManager.Instance.어싱크로띄우기(ViewID.VillageToExploreView);
        }
    }
    
    public class VillageToExploreInteractionPresenter : PresenterBase<VillageToExploreInteractionView>
    {
        public VillageToExploreInteractionPresenter(IView view) : base(view)
        { }
        
        public override UniTask EnterAction(CancellationToken token)
        {
            //모델 = InputAction.
            return UniTask.CompletedTask;
        }

        public override UniTask ExitAction(CancellationToken token)
        {
            return UniTask.CompletedTask;
        }
    }
}