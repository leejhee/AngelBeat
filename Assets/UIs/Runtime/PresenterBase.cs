using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace UIs.Runtime
{
    
    public abstract class PresenterBase<TView> : IPresenter where TView : class, IView
    {
        public IReadOnlyList<IUIModel> Models { get; }
        IView IPresenter.View { get; }
        protected TView View { get; private set; }
        protected object Payload { get; private set; }
        private bool _initialized, _bound;

        public void Initialize(IView view, object payload = null)
        {
            if (_initialized) return;
            View = view as TView ?? throw new System.InvalidOperationException(
                $"{GetType().Name}: View type must be {typeof(TView).Name}");
            Payload = payload;
            _initialized = true;
            OnInitialized();          // 필요 없으면 비워둬도 됨
        }

        public void Bind()
        {
            if (!_initialized) throw new System.InvalidOperationException("Bind before Initialize");
            if (_bound) return;
            _bound = true;
            OnBind();                 // 이벤트 구독 등
            OnFirstRender();          // 현재 상태 즉시 그리기
        }

        public void Dispose()
        {
            OnDispose();              // 구독 해제 등
            _bound = false;
            _initialized = false;
            View = null;
            Payload = null;
        }

        // 파생에서 필요한 것만 오버라이드
        protected virtual void OnInitialized() { }
        protected virtual void OnBind() { }
        protected virtual void OnFirstRender() { }
        protected virtual void OnDispose() { }
    }
}