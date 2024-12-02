using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapPopup : UI_Popup
{
    enum GameObjects 
    {
        Map
    }

    public override void Init()
    {
        base.Init();
        Bind<GameObject>(typeof(GameObjects));
    }
}
