using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ExploreScene : UI_Scene
{
    enum Buttons
    {
        ShowMapPopup,
    }

    public override void Init()
    {
        base.Init();
        Bind<Button>(typeof(Buttons));
        BindButton();
    }

    void BindButton()
    {
        BindEvent(GetButton((int)Buttons.ShowMapPopup).gameObject, OnClickMapGenerate);
    }

    void OnClickMapGenerate(PointerEventData evt)
    {
        UIManager.Instance.ShowPopupUI<MapViewPopup>();
    }
}
