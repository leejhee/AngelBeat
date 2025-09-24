 using Cysharp.Threading.Tasks;
 using System;
 using System.Threading;

 namespace UIs.Runtime
 {

     public abstract class PresenterBase<TView> : IPresenter where TView : class, IView
     {
         protected readonly TView View;
         protected readonly PresenterEventBus ViewEvents = new();
         protected readonly PresenterEventBus ModelEvents = new();
         protected CancellationTokenSource Cts;

         protected PresenterBase(IView view)
         {
             View = view as TView??
                    throw new ArgumentException(
                        $"Presenter expects view {typeof(TView).Name} but got null");
         }

         #region IPresenter Implementation

         public virtual void OnPause() { }
         public virtual void OnResume() { }

         public virtual async UniTask OnEnterAsync(CancellationToken token)
         {
             Cts = new CancellationTokenSource();
             await EnterAction(token);
         }

         public virtual async UniTask OnExitAsync(CancellationToken token)
         {
             Cts?.Cancel();
             Cts?.Dispose();
             Cts = null;
             ViewEvents.Clear();
             ModelEvents.Clear();
             await ExitAction(token);
         }

         public virtual void Dispose()
         {
             ViewEvents.Dispose();
             ModelEvents.Dispose();
         }

         #endregion

         public abstract UniTask EnterAction(CancellationToken token);
         public abstract UniTask ExitAction(CancellationToken token);
     }
 }