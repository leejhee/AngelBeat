using Core.Scripts.Foundation.Define;
using GamePlay.Common.Scripts.Entities.Character;
using GamePlay.Common.Scripts.Scene;
using GamePlay.Features.Battle.Scripts;
using UnityEngine;

namespace GamePlay.Features.Explore.Scripts.Symbol.Encounter
{
    
    public class ExploreBattleSymbol : MonoBehaviour
    {
        [SerializeField] private SystemEnum.Dungeon dungeon;
        [SerializeField] private string mapName;
        private void OnTriggerEnter2D(Collider2D other)
        {
            ExploreController player = other.GetComponent<ExploreController>();
            if (!player) return;
            
            Party party = ExploreManager.Instance.playerParty;
            #region Move to Battle Scene
            
            //TODO : 추후 씬 트랜지션 지침 받고 이 부분에 기입할 것
            
            BattlePayload.Instance.SetBattleData(party, dungeon, mapName);
            ExplorePayload.Instance.SetContinueExplore(dungeon, 1, party, player.transform.position);
            
            GamePlaySceneUtil.LoadBattleScene();
            #endregion
        }
    }
}
