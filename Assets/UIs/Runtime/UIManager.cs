using Core.Scripts.Managers;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace UIs.Runtime
{
    public enum UILayer { Screen, Modal, Toast }
    
    /// <summary>
    /// 초기화가 필요하지 않으므로, 우리 UIManager은 static 클래스로만 존재합니다.
    /// </summary>
    public static class UIManager 
    {
        private sealed class Route
        {
            public object AddressableKey;
            public Type ModelType;
            public Func<IPresenter> PresenterFactory;
            public UILayer Layer;
        }

        private static readonly Dictionary<string, Route> Routes = new();
        private static readonly Stack<IPresenter> Stack = new();

        public static void RegisterAddressable<TModel>(
            string route,
            object key,
            Func<PresenterBase<TModel>> factory,
            UILayer layer = UILayer.Screen)
            where TModel : IUIModel
        {
            Routes[route] = new Route
            {
                AddressableKey = key, ModelType = typeof(TModel), PresenterFactory = factory, Layer = layer
            };
        }

        private static Transform GetParent(UILayer layer) => layer switch
        {
            UILayer.Modal => LayerUtil.ModalLayer,
            UILayer.Toast => LayerUtil.ToastLayer,
            _ => LayerUtil.ScreenLayer
        };
        
        public static async UniTask<IPresenter> OpenAsync<TModel>(
            string route,
            TModel model,
            UILayer? layerOverride = null,
            CancellationToken ct = default)
            where TModel : IUIModel
        {
            if (!Routes.TryGetValue(route, out var r))
                throw new InvalidOperationException($"Unregistered route: {route}");
            Type modelType = typeof(TModel);
            if(r.ModelType != modelType && !r.ModelType.IsAssignableFrom(modelType))
                throw new InvalidOperationException($"Route {route} expects {r.ModelType.Name}, got {modelType.Name}");
            
            var parent = GetParent(layerOverride ?? r.Layer);
            var go = await ResourceManager.Instance.InstantiateAsync(r.AddressableKey, parent, false, ct);
            var view = go.GetComponent<IView<TModel>>();
            if (view == null)
                throw new InvalidOperationException(
                    $"View with IView<{modelType.Name}> not found on prefab: {go.name}");
            var p = r.PresenterFactory() as PresenterBase<TModel>;
            p.AttachView(view);
            p.Show(model);
            Stack.Push(p);
            return p;
        }

        public static void Back()
        {
            if (Stack.Count == 0) return;
            var p = Stack.Pop();
            p.OnBackRequested();
            p.Dispose();
        }

        public static void CloseTop()
        {
            if (Stack.Count == 0) return;
            var p = Stack.Pop();
            p.Hide();
            p.Dispose();
        }
        
        public static void CloseAll()
        {
            while (Stack.Count > 0) CloseTop();
        }
    }
}