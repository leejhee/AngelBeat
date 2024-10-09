using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_Info : UI_Scene
{
    enum Texts
    {
        SelectedPositionText,
        CursorPositionText
    }

    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    public override void Init()
    {
        base.Init();
        Bind<TMP_Text>(typeof(Texts));
    }


}
