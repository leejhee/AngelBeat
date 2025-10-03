using Core.Scripts.Foundation.Define;
using Core.Scripts.Foundation.SceneUtil;
using GamePlay.Common.Scripts.Entities.Character;
using GamePlay.Features.Battle.Scripts;
using GamePlay.Features.Scripts.Explore;
using UnityEngine;

namespace GamePlay.Features.Explore.Scripts
{
    
    public class ExploreBattleSymbol : MonoBehaviour
    {
        [SerializeField] private SystemEnum.Dungeon dungeon;
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            ExploreController player = other.GetComponent<ExploreController>();
            Party party = player.PlayerParty;
            
            #region Move to Battle Scene
            BattlePayload.Instance.SetBattleData(party, dungeon);
            //TODO : 씬 연출 이펙트. 필요한가?
            SceneLoader.LoadSceneWithLoading(SystemEnum.eScene.BattleTestScene, null);
            Debug.Log("씬 로딩 완료!");
            #endregion
        }
    }
}
