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
        PlayTutorial_1();
    }

    private async void PlayTutorial_1()
    {
        await NovelManager.InitAsync();
        NovelManager.Instance.PlayTutorial(1);

    }
    // public void OnButtonClick()
    // {
    //     
    //     NovelManager.Instance.PlayScript("Tutorial_2");
    // }


}
