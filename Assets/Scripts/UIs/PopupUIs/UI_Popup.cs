using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UI_Popup : UI_Base
{
    public override void Init()
    {
        base.Init();
        UIManager.Instance.SetCanvas(gameObject, true);
    }

    public virtual void ReOpenPopupUI() { }
}
