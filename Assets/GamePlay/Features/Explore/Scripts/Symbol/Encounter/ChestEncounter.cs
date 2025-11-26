using GamePlay.Features.Explore.Scripts;
using System.Collections;
using System.Collections.Generic;
using UIs.Runtime;
using UnityEngine;

public class ChestEncounter : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        ExploreController player = other.GetComponent<ExploreController>();
        if (!player) return;

        _ = UIManager.Instance.ShowViewAsync(ViewID.ExploreChestPopup);
            
        Destroy(gameObject);
    }
}
