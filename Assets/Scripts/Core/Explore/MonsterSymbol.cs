using AngelBeat.Core.Character;
using AngelBeat.Scene;
using AngelBeat.Core.SingletonObjects;
using System;
using UnityEngine;

namespace AngelBeat.Core.Explore
{
    public class MonsterSymbol : MonoBehaviour
    {
        [SerializeField] private SystemEnum.eDungeon dungeon;
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            ExploreController player = other.GetComponent<ExploreController>();
            Party party = player.PlayerParty;

            BattlePayload.Instance.SetBattleData(party, dungeon);
            
            SceneLoader.LoadSceneWithLoading(SystemEnum.eScene.BattleTestScene, null);
            Debug.Log("씬 로딩 완료!");

        }
    }
}
