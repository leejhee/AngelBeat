using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Core.Foundation.Define.SystemEnum;

public abstract class UI_Base : MonoBehaviour
{
    protected Dictionary<Type, UnityEngine.Object[]> _objects = new Dictionary<Type, UnityEngine.Object[]>();

    float setWidth = 3200;
    float setHeight = 1440;
    private CanvasScaler canvasScaler;
    public virtual void Init()
    {
        canvasScaler = GetComponent<CanvasScaler>();
        
    }
    private void Awake()
    {
        Init();
    }

    //protected void Bind<T>(Type type) where T : UnityEngine.Object
    //{
    //    string[] names = Enum.GetNames(type);
    //    UnityEngine.Object[] objects = new UnityEngine.Object[names.Length];
    //    _objects.Add(typeof(T), objects); // Dictionary 에 추가

    //    // T 에 속하는 오브젝트들을 Dictionary의 Value인 objects 배열의 원소들에 하나하나 추가
    //    for (int i = 0; i < names.Length; i++)
    //    {
    //        if (typeof(T) == typeof(GameObject))
    //            objects[i] = Util.FindChild(gameObject, names[i], true);
    //        else
    //            objects[i] = Util.FindChild<T>(gameObject, names[i], true);

    //        if (objects[i] == null)
    //            Debug.Log($"Failed to bind({names[i]})");
    //    }
    //}

    //protected T Get<T>(int idx) where T : UnityEngine.Object
    //{
    //    UnityEngine.Object[] objects = null;
    //    if (_objects.TryGetValue(typeof(T), out objects) == false)
    //        return null;

    //    return objects[idx] as T;
    //}
    //protected GameObject GetGameObject(int idx) { return Get<GameObject>(idx); }
    //protected TMP_Text GetText(int idx) { return Get<TMP_Text>(idx); }
    //protected Button GetButton(int idx) { return Get<Button>(idx); }
    //protected Image GetImage(int idx) { return Get<Image>(idx); }

    //public static void BindEvent(GameObject go, Action<PointerEventData> action, UIEvent type = UIEvent.Click)
    //{
    //    UI_EventHandler evt = Util.GetOrAddComponent<UI_EventHandler>(go);

    //    switch (type)
    //    {
    //        case UIEvent.Click:
    //            evt.OnClickHandler -= action;
    //            evt.OnClickHandler += action;
    //            break;
    //        case UIEvent.Drag:
    //            evt.OnDragHandler -= action;
    //            evt.OnDragHandler += action;
    //            break;
    //    }
    //}

    public void SetResolution()
    {
        float deviceWidth = Screen.width;
        float deviceHeight = Screen.height;

        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(setWidth, setHeight);
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

        if(setWidth / setHeight < deviceWidth / deviceHeight)
        {
            canvasScaler.matchWidthOrHeight = 1f;
        }
        else
        {
            canvasScaler.matchWidthOrHeight = 0f;

        }
    }
}
