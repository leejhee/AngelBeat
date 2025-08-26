// using System.Collections.Generic;
// using UnityEngine;
// using AngelBeat.UI;
// using Character.Unit;
// using Core.Foundation;
// using Core.Foundation.Utils;
// using System;
//
// namespace AngelBeat
// {
//     public class UIManager : SingletonObject<UIManager>
//     {
//         int _order = 0;
//
//         Stack<UI_Popup> _popupStack = new Stack<UI_Popup>();
//         
//         Dictionary<Type, GameObject> _popupInstances = new Dictionary<System.Type, GameObject>();
//
//         UI_Scene _scene = null;
//         
//         #region 생성자
//         UIManager() { }
//         #endregion
//
//         public GameObject Root
//         {
//             get
//             {
//                 GameObject root = GameObject.Find("@UI_Root");
//                 if (root == null)
//                     root = new GameObject { name = "@UI_Root" };
//
//                 return root;
//             }
//         }
//         
//         
//         public void ShowUI(GameObject UIObject)
//         {
//             if (UIObject)
//             {
//                 ResourceManager.Instance.Instantiate(UIObject, Root.transform);
//             }
//         }
//
//         public void ShowFloatingUI(CharBase ch, FloatingUI ui)
//         {
//             if (!ui)
//             {
//                 Debug.LogError("Check your floating ui prefab.");
//                 return;
//             }
//             ResourceManager.Instance.Instantiate(ui.gameObject, ch.FloatingUIRoot);
//         }
//         
//
//         public T ShowPopupUI<T>(string name = null) where T : UI_Popup
//         {
//             if (string.IsNullOrEmpty(name))
//                 name = typeof(T).Name;
//             GameObject popup;
//             T popupUI;
//
//             if(_popupInstances.TryGetValue(typeof(T), out popup) == false)
//             {
//                 popup = ResourceManager.Instance.Instantiate($"UI/popup/{name}");
//                 _popupInstances.Add(typeof(T), popup);
//                 popupUI = Util.GetOrAddComponent<T>(popup);
//             }
//             else
//             {
//                 popupUI = Util.GetOrAddComponent<T>(popup);
//                 popupUI.ReOpenPopupUI();
//                 popupUI.GetComponent<Canvas>().sortingOrder = _order++;
//             }
//
//             _popupStack.Push(popupUI);
//             popup.transform.SetParent(Root.transform);
//             return popupUI;
//         }
//
//         public void ClosePopupUI()
//         {
//             if (_popupStack.Count <= 0)
//                 return;
//             UI_Popup popup = _popupStack.Pop();
//             popup.gameObject.SetActive(false);
//             _order--;
//         }
//
//         public void CloseAllPopupUI()
//         {
//             while (_popupStack.Count > 0)
//                 ClosePopupUI();
//         }
//
//         public void SetCanvas(GameObject go, bool sort = true, int order = 0)
//         {
//             Canvas canvas = Util.GetOrAddComponent<Canvas>(go);
//             canvas.renderMode = RenderMode.ScreenSpaceOverlay;
//             canvas.overrideSorting = true;
//
//             if (sort)
//                 canvas.sortingOrder = _order++;
//             else
//                 canvas.sortingOrder = order;
//         }
//
//         public T ShowSceneUI<T>(string name = null) where T : UI_Scene
//         {
//             if (string.IsNullOrEmpty(name))
//                 name = typeof(T).Name;
//
//             GameObject go = ResourceManager.Instance.Instantiate($"UI/Scene/{name}");
//             T sceneUI = Util.GetOrAddComponent<T>(go);
//             _scene = sceneUI;
//
//             go.transform.SetParent(Root.transform);
//
//             return sceneUI;
//         }
//
//         public T MakeSubItem<T>(Transform parent = null, string name = null) where T : UI_Base
//         {
//             if (string.IsNullOrEmpty(name))
//                 name = typeof(T).Name;
//
//             GameObject go = ResourceManager.Instance.Instantiate($"UI/SubItem/{name}");
//             if (parent != null)
//                 go.transform.SetParent(parent);
//
//
//             return Util.GetOrAddComponent<T>(go);
//         }
//
//         public T FindSceneUI<T>(string name = null) where T : UI_Scene
//         {
//             if(string.IsNullOrEmpty(name))
//                 name = typeof(T).Name;
//
//             T sceneUI = GameObject.FindAnyObjectByType<T>();
//             return sceneUI;
//         }
//
//         public T FindPopupUI<T>(string name = null) where T : UI_Popup
//         {
//             if (string.IsNullOrEmpty(name))
//                 name = typeof(T).Name;
//
//             T popupUI = GameObject.FindAnyObjectByType<T>();
//             return popupUI;
//         }
//
//         public void Clear()
//         {
//             CloseAllPopupUI();
//         }
//     }
//
// }
