using System.Collections.Generic;
using UnityEngine;
using UIs.Runtime;

namespace GamePlay.Features.UI.Interaction
{
    // 가이드 관련 string도 데이터화하는게 맞나? 추후 결정 예정.
    // 데이터화되면, 이제 데이터로부터 symbol의 '이름'에 따라 가져올 예정.
    
    /// <summary>
    /// 저장 X. 탐사 내 상호작용 오브젝트일 경우, 범위 내에 접근 시 뜨는 팝업
    /// </summary>
    public class ExploreInteractionPopup
    {
        [SerializeField] private Transform interactionInputGuide;
        [SerializeField] private List<string> guidePrompts;
        [SerializeField] private GameObject buttonPrefab; // 필요 시 사용
        
        
    }
}