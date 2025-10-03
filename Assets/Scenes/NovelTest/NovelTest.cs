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
    [SerializeField] private UnityEngine.UI.Button button;
    // Start is called before the first frame update
    void Start()
    {
        PlayScript();
    }

    private async void PlayScript()
    {
        
        await NovelManager.InitAsync();
        NovelManager.Instance.PlayScript("Tutorial_1");

    }
    public void OnButtonClick()
    {
        
        NovelManager.Instance.PlayScript("Tutorial_2");
    }


}
