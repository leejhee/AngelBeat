using System;
using UnityEngine;

namespace UIs.Runtime
{
    /// <summary>
    /// View가 의존할 모델에 대한 마커 인터페이스
    /// </summary>
    public interface IUIModel {}
    
    public interface IView<in TModel> where TModel : IUIModel
    {
        void BindObject(TModel vm); // 모델에 대한 이벤트를 프레젠터에 구독.

        void Show();    // 해당 View를 렌더링합니다.
        void Hide();    // 해당 View를 숨깁니다.
        void Close();   // 해당 View를 닫습니다.
        
        event Action OnBackRequested;   // 뒤로가기에 대한 콜백
        RectTransform Rect { get; }     // 해당 View의 RectTransform.
    }
}