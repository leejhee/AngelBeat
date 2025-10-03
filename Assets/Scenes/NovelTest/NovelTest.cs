using novel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UIs.Runtime;

public class NovelTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        PlayScript();
    }

    private async void PlayScript()
    {
        await NovelManager.InitAsync();
        if (NovelManager.act == 1)
        {
            NovelManager.Instance.PlayScript("Tutorial_1");
        }
        else if (NovelManager.act == 2)
        {
            NovelManager.Instance.PlayScript("Tutorial_2");

        }
        else if (NovelManager.act == 3)
        {
            NovelManager.Instance.PlayScript("Tutorial_3");

        }

    }
    public void OnButtonClick()
    {
        
        NovelManager.Instance.PlayScript("Tutorial_2");
    }


}
