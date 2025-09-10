using System;
using System.Threading;

namespace UIs.Runtime
{
    public interface IRouteReceiver
    {
        void SetRoute(string route);
    }
    
    public abstract class PresenterBase<TModel> : IPresenter, IRouteReceiver where TModel : IUIModel
    {
        protected IView<TModel> View;       // Presenter은 View를 참조한다.
        protected TModel        Model;      // Presenter은 Model을 참조한다.
        protected UILayer       Layer;      // 어느 레이어에 존재?

        private CancellationTokenSource _cts;
        protected CancellationToken ViewToken => _cts?.Token ?? CancellationToken.None;
        private bool _disposed = false;
        
        /// <summary>
        /// UIManager에서 Presenter 생성 시 참조하는 View
        /// </summary>
        /// <param name="view"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void AttachView(IView<TModel> view)
        {
            View = view ?? throw new ArgumentNullException(nameof(view));
            View.OnBackRequested += OnBackRequested;
        }
        
        public void Show(TModel model)
        {
            RestartCTS();
            Model = model;
            View.BindObject(model);
            View.Show();
        }
        
        /// <summary>
        /// 보이는 상태에서 Model만 바뀌는 경우에 사용(보통 입력에 의해 바뀜)
        /// </summary>
        /// <param name="model">새로이 갱신되어 반영될 모델</param>
        public void Refresh(TModel model)
        {
            Model = model;
            View.BindObject(model);
        }
        
        public void Hide()
        {
            _cts?.Cancel();
            View?.Hide();
        }
        
        public virtual void OnBackRequested()
        {
            Hide();
        }

        public virtual void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            try
            {
                if(View != null)
                    View.OnBackRequested -= OnBackRequested;
            }
            catch { /*TODO : Destroying Order Concern*/}
            
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;

            try
            {
                View?.Close();
            }
            catch { /*TODO : Already Destroyed*/ }
            
            View  = null; // 연결 해제
            Model = default!;
        }

        public void SetRoute(string route)
        {
            throw new System.NotImplementedException();
        }

        private void RestartCTS()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
        }
    }
}