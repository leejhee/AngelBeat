using AngelBeat.Core.Character;
using AngelBeat.Scene;
using AngleBeat.Core.SingletonObjects;
using System;
using UnityEngine;

namespace AngelBeat.Core.Explore
{
    public class MonsterSymbol : MonoBehaviour
    {
        //TODO : dungeon을 통해 선별할 맵 프리팹 풀을 골라야 한다.
        [SerializeField] private SystemEnum.eDungeon dungeon;
        
        private async void OnTriggerEnter2D(Collider2D other)
        {
            try
            {
                ExploreController player = other.GetComponent<ExploreController>();
                Party party = player.PlayerParty;

                BattlePayload.Instance.SetBattleData(party, dungeon);
            
                Debug.Log("나중에는 로딩으로 하세요~ 씬 바꿈.");
                await SceneUtil.LoadSceneAdditiveAsync("BattleTestScene");
                Debug.Log("씬 로딩 완료!");
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }
    }
}
