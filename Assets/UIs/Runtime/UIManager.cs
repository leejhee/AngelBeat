// using Core.Scripts.Managers;
// using Cysharp.Threading.Tasks;
// using System;
// using System.Collections.Generic;
// using System.Threading;
// using UnityEngine;
//
// namespace UIs.Runtime
// {
//     public enum UILayer { Screen, Modal, Toast }
//     
//     /// <summary>
//     /// 초기화가 필요하지 않으므로, 우리 UIManager은 static 클래스로만 존재합니다.
//     /// </summary>
//     public static class UIManager 
//     {
//         private sealed class Route
//         {
//             public object AddressableKey;
//             public UILayer Layer;
//         }
//
//         private static readonly Dictionary<UIView, Route> Routes = new();
//         private static readonly Stack<IPresenter> Stack = new();
//
//         /// <summary>
//         /// SO 등록
//         /// </summary>
//         /// <param name="key"> Addressable Key</param>
//         /// <param name="token"> 취소 토큰</param>
//         public static async void RegisterAddressable(object key, CancellationToken token = default)
//         {
//             UIViewList list = await ResourceManager.Instance.LoadAsync<UIViewList>(key, token);
//             foreach (UIViewList.ViewEntry e in list.ViewTable)
//                 Routes[e.view] = new Route { Layer = e.layer, AddressableKey = e.viewReference };
//         }
//
//         private static Transform GetParent(UILayer layer) => layer switch
//         {
//             UILayer.Modal => LayerUtil.ModalLayer,
//             UILayer.Toast => LayerUtil.ToastLayer,
//             _ => LayerUtil.ScreenLayer
//         };
//         
//         public static async UniTask<IView> OpenAsync(
//             UIView view,
//             UILayer? layerOverride = null,
//             CancellationToken ct = default)
//         {
//             if (!Routes.TryGetValue(view, out Route r))
//                 throw new InvalidOperationException($"Unregistered View: {view}");
//
//             Transform parent = GetParent(layerOverride ?? r.Layer);
//             GameObject go = await ResourceManager.Instance.InstantiateAsync(r.AddressableKey, parent, false, ct);
//             //PresenterBase를 가져와야함.
//             //PresenterBase에다가 Model도 가져와서 바인딩하고 저 Stack에 넣어야겠네...
//         }
//
//         //public static void Back()
//         //{
//         //    if (Stack.Count == 0) return;
//         //    var p = Stack.Pop();
//         //    p.OnBackRequested();
//         //    p.Dispose();
//         //}
// //
//         //public static void CloseTop()
//         //{
//         //    if (Stack.Count == 0) return;
//         //    var p = Stack.Pop();
//         //    p.Hide();
//         //    p.Dispose();
//         //}
//         //
//         //public static void CloseAll()
//         //{
//         //    while (Stack.Count > 0) CloseTop();
//         //}
//     }
// }