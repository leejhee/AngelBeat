using System;
using System.Collections.Generic;
using UnityEngine;

namespace UIs.Runtime
{
    public enum UILayer { Screen, Modal, Toast }
    public interface IModelFactory
    {
        
    }
    
    /// <summary>
    /// UI 모델에 대한 마커 인터페이스
    /// </summary>
    public interface IUIModel {}

    /// <summary>
    /// UI 뷰를 의미하며, UI 관련 변화만 기재할 것.
    /// </summary>
    public interface IView
    {
        event Action OnClose;
        Transform Root { get; }
    }
    
    /// <summary>
    /// UI Presenter을 의미하며, Model의 변경 및 View 메서드 호출의 역할.
    /// </summary>
    public interface IPresenter : IDisposable
    {
        IReadOnlyList<IUIModel> Models { get; } // 복수의 모델이 영향을 미칠 수 있음.
        IView View { get; } // 프레젠터와 뷰는 1 : 1
        void Initialize(IView view, object payload);
        void Bind();
    }
    
    /// <summary>
    /// 어떤 모델을 받아올건지 선택하는 용도의 enum
    /// </summary>
    public enum UIModel
    {
        GlobalSave,
        KeyMap,
        
        Party,
        Inventory,
        
    }
    
    /// <summary>
    /// View를 SO에 매핑하기 위한 enum
    /// </summary>
    public enum UIView
    {
        ExploreGuideView,
        ExploreSelectionView,
        ExploreSceneView,
        BattleSceneView,
        
    }

    public static class PresenterFactory
    {
        
    }
    
}