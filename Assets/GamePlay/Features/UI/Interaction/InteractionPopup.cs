using UnityEngine;

namespace GamePlay.Features.UI.Interaction
{
    /// <summary>
    /// 저장 X. 탐사 내 상호작용 오브젝트일 경우, 범위 내에 접근 시 뜨는 팝업
    /// </summary>
    public class ExploreInteractionPopup : MonoBehaviour
    {
        [SerializeField] private Transform interactionInputGuide;
    }
}