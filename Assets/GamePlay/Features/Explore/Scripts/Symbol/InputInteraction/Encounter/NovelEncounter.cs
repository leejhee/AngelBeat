using System;
using Unity.VisualScripting;
using UnityEngine;

namespace GamePlay.Features.Explore.Scripts.Symbol.InputInteraction.Encounter
{
    public class NovelEncounter : MonoBehaviour
    {
        [SerializeField] private string novelTitle;
        private void PlayNovel()
        {
            _ = NovelManager.PlayScriptAndWait(novelTitle);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            
        }
    }
}
