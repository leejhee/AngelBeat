using System;
using UnityEngine;

namespace GamePlay.Common.Scripts.Tutorial
{
    public class NewGameInitialTutorial : MonoBehaviour
    {
        private async void Start()
        {
            await NovelManager.Instance.PlayScript("1");
            NovelManager.Player.OnScriptEnd += 
                async() => await NovelManager.Instance.PlayScript("2");
        }
    }
}