using Core.Scripts.UIAbstraction;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace UIs.Runtime
{
    public class UINavigator : MonoBehaviour
    {
        public Transform screenLayer;
        public Transform modalLayer;
        public Transform toastLayer;
        public MonoBehaviour viewFactoryBehaviour;
        
        private IViewFactory ViewFactory => viewFactoryBehaviour as IViewFactory;
        private readonly Stack<IPresenter> _presenterStack = new();

        private Transform Root(int layer) => 
            layer >= 20 ? (toastLayer ? toastLayer : screenLayer) :
            layer >= 10 ? (modalLayer != null? modalLayer : screenLayer) : screenLayer;
        
        /// <summary>
        /// UI Presenter을 생성해서 stack에 푸쉬, Presenter에 달린 View들을 보여준다.
        /// </summary>
        public async UniTask<IPresenter> PushPresenterAsync
            (string route, object viewModel = null, CancellationToken ct = default)
        {
            var p = await ViewFactory.CreatePresenterAsync(route, screenLayer, ct);
            if (p == null) return null;
            ((Component)p).transform.SetParent(Root(p.Layer), false);
            if(_presenterStack.TryPeek(out var top)) 
                top.OnFocusLost(); //원래 위에 있던 걸 가리고
            (p as IView)?.BindObject(viewModel);
            if (p is IUniPresenter up)
                await up.ShowUniAsync(ct);
            else
                await p.ShowAsync(ct); //지금 만든걸 켜준다.
            p.OnFocusGained();      //포커스가 바뀌었으므로 콜백을 해준다.
            _presenterStack.Push(p);
            return p;
        }
        
        /// <summary>
        /// presenter의 stack에서 top에 있는 UI를 pop하고 그 아래 Layer을 보여주다.
        /// </summary>
        public async UniTask PopAsync(CancellationToken ct=default) {
            if (_presenterStack.Count==0) return; 
            var top=_presenterStack.Pop();
            if (top is IUniPresenter up) 
                await up.HideUniAsync(ct); 
            else 
                await top.HideAsync(ct);
            Destroy(((Component)top).gameObject);
            if (_presenterStack.TryPeek(out var next)) 
            { 
                if (!next.IsVisible) {
                    if (next is IUniPresenter un) 
                        await un.ShowUniAsync(ct); 
                    else 
                        await next.ShowAsync(ct);
                } 
                next.OnFocusGained(); 
            }
        }
        
        public async UniTask<bool> BackAsync(CancellationToken ct=default) {
            if (_presenterStack.Count==0) 
                return false; 
            var top=_presenterStack.Peek();
            if (top.OnBackRequested()) 
                return true; 
            await PopAsync(ct); 
            return true;
        }
        
        public async UniTask CloseAllAsync(CancellationToken ct = default)
        {
            while(_presenterStack.Count>0) 
                await PopAsync(ct); 
        }
    }
}