using UIs.Runtime;
using UnityEngine;

namespace GamePlay.Features.Explore.Scripts.Symbol.Encounter
{
    public class HerbEncounter : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            ExploreController player = other.GetComponent<ExploreController>();
            if (!player) return;

            _ = UIManager.Instance.ShowViewAsync(ViewID.ExploreHerbPopup);
            
            Destroy(gameObject);
        }
    }
}
