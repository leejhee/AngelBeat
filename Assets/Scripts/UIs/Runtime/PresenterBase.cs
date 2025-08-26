using Core.UIAbstraction;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace UIs.Runtime
{
    public interface IRouteReceiver
    {
        void SetRoute(string route);
    }
    
    public abstract class PresenterBase<TVM> : MonoBehaviour,
        IUniPresenter, IPresenter, IView<TVM>, IRouteReceiver
    {
        [SerializeField] protected CanvasGroup canvasGroup;
        [SerializeField] protected float fadeIn = 0.1f;
        [SerializeField] protected float fadeOut = 0.1f;
        [SerializeField] protected int layer = 0; // 0:Screen, 10:Modal, 20:Toast
        
        public string Route { get; private set; }
        public bool IsVisible { get; private set; }
        public int Layer => layer;
        protected TVM VM { get; private set; }
        
        public void SetRoute(string route) => Route = route;
        
        public void BindObject(TVM vm)
        {
            VM = vm; Render(vm);
        }

        public void BindObject(object vm) => BindObject(vm is TVM t ? t : default);
        protected abstract void Render(TVM vm);


        public virtual async UniTask ShowUniAsync(CancellationToken ct = default)
        {
            gameObject.SetActive(true);
            if (canvasGroup)
            {
                canvasGroup.blocksRaycasts = true;
                float t = 0f;
                while (t < fadeIn)
                {
                    ct.ThrowIfCancellationRequested();
                    t += Time.unscaledDeltaTime;
                    canvasGroup.alpha = fadeIn <= 0 ? 1f : Mathf.Clamp01(t / fadeIn);
                    await UniTask.Yield(PlayerLoopTiming.Update, ct);
                }

                canvasGroup.alpha = 1f;
            }

            IsVisible = true;
            OnAfterShow();
        }

        public virtual async UniTask HideUniAsync(CancellationToken ct = default)
        {
            if (canvasGroup)
            {
                canvasGroup.blocksRaycasts = false;
                float t = fadeOut;
                while (t > 0f)
                {
                    ct.ThrowIfCancellationRequested();
                    t -= Time.unscaledDeltaTime;
                    canvasGroup.alpha = fadeOut <= 0 ? 0f : Mathf.Clamp01(t / fadeOut);
                    await UniTask.Yield(PlayerLoopTiming.Update, ct);
                }
            }
        }


        public Task ShowAsync(CancellationToken ct) => ShowUniAsync(ct).AsTask();
        public Task HideAsync(CancellationToken ct) => HideUniAsync(ct).AsTask();


        public virtual void OnFocusGained() { }
        public virtual void OnFocusLost() { }
        public virtual bool OnBackRequested() => false;
        
        protected virtual void OnAfterShow() { }
        protected virtual void OnAfterHide() { }
    }
}