using Core.Scripts.Foundation.Define;
using Core.Scripts.Foundation.Utils;
using GamePlay.Common.Scripts.Entities.Character;
using GamePlay.Common.Scripts.Scene;
using GamePlay.Features.Battle.Scripts;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay.Features.Explore.Scripts.Symbol.Encounter
{
    
    public class ExploreBattleSymbol : MonoBehaviour
    {
        [SerializeField] private SystemEnum.Dungeon dungeon;
        [SerializeField] private List<string> mapNames;
        
        [Header("랜덤 모드 옵션")]
        [Tooltip("true면 mapNames에서 랜덤으로 뽑아서 전투 시퀀스를 구성합니다.\nfalse면 mapNames 순서를 그대로 사용합니다.")]
        [SerializeField] private bool useRandomOrder = true;

        [Tooltip("랜덤 모드일 때 몇 번의 전투를 진행할지")]
        [SerializeField] private int randomBattleCount = 2;

        private GameRandom random; //temporary
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            ExploreController player = other.GetComponent<ExploreController>();
            if (!player) return;
            
            Party party = ExploreManager.Instance.playerParty;
            if (party == null)
            {
                Debug.LogError("[ExploreBattleSymbol] Player party is null.");
                return;
            }
            
            var stageList = BuildStageList();
            if (stageList == null || stageList.Count == 0)
            {
                Debug.LogError("[ExploreBattleSymbol] Stage list is empty. 전투를 시작할 수 없습니다.");
                return;
            }
            #region Move to Battle Scene
            
            //TODO : 추후 씬 트랜지션 지침 받고 이 부분에 기입할 것
            
            BattlePayload.Instance.SetBattleData(party, dungeon, mapNames);
            ExplorePayload.Instance.SetContinueExplore(dungeon, 1, party, player.transform.position);
            
            GamePlaySceneUtil.LoadBattleScene();
            #endregion
        }
        
        private List<string> BuildStageList()
        {
            var result = new List<string>();

            if (mapNames == null || mapNames.Count == 0)
            {
                Debug.LogWarning("[ExploreBattleSymbol] mapNames가 비어 있습니다.");
                return result;
            }

            if (!useRandomOrder)
            {
                // 지정된 순서대로
                result.AddRange(mapNames);
                return result;
            }
            
            // gamerandom은 보류함
            if (random == null)
            {
                random = new GameRandom();
            }

            int count = Mathf.Max(1, randomBattleCount);
            for (int i = 0; i < count; i++)
            {
                int idx = random.Next(mapNames.Count);
                result.Add(mapNames[idx]);
            }
            
            
            return result;
        }
    }
}
