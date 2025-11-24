using System;
using Unity.VisualScripting;
using UnityEngine;

namespace GamePlay.Features.Explore.Scripts.Symbol.InputInteraction.Encounter
{
    public class NovelEncounter : MonoBehaviour
    {
        [SerializeField] private string novelTitle;
        private async void PlayNovel()
        {
            await NovelManager.PlayScriptAndWait(novelTitle);
            
            Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            ExploreController player = other.GetComponent<ExploreController>();
            if (!player) return;
            
            PlayNovel();
            
            
        }
    }
}
