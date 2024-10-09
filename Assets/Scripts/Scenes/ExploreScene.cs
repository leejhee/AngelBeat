using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExploreScene : BaseScene
{
    protected override void Init()
    {
        base.Init();

        GameManager.UI.ShowSceneUI<UI_Info>();
    }

    public override void Clear()
    {
        throw new System.NotImplementedException();
    }

}
