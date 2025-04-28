using AngelBeat.Scene;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AngelBeat.Core.Explore
{
    public class MonsterSymbol : MonoBehaviour
    {
        private async void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log("나중에는 로딩으로 하세요~ 씬 바꿈.");
            await SceneUtil.LoadSceneAdditiveAsync("BattleTestScene");
        }
    }
}
