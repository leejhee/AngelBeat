using AngelBeat.Core.Character;
using AngelBeat.Core.Map;
using AngelBeat.Scene;
using AngelBeat.Core.SingletonObjects;
using System.Collections.Generic;
using UnityEngine;

namespace AngelBeat.Core.Battle
{
    public class BattleController : MonoBehaviour
    {
        private List<CharBase> _battleCharList;
        private TurnController _turnManager;

        private void Start()
        {
            BattleInitialize();
        }
        
        // TODO : Initializer 객체 따로 사용해서 초기화할 것
        private void BattleInitialize()
        {
            Debug.Log("Starting Battle Initialization...");
            SystemEnum.eDungeon dungeon = BattlePayload.Instance.DungeonName;
            StageField battleField = SetMapEnvironment(dungeon);
            if (!battleField)
            {
                Debug.LogError("Map Load Error");
                return;
            }
            
            Party party = BattlePayload.Instance.PlayerParty;
            List<CharBase> battleMembers = battleField.SpawnAllUnits(party);
            _turnManager = new TurnController(battleMembers); 
            _turnManager.ChangeTurn();
            
            Debug.Log("Battle Initialization Complete");
            BattlePayload.Instance.Clear();
            
        }

        private StageField SetMapEnvironment(SystemEnum.eDungeon dungeon)
        {
            BattleFieldGroup battleFieldGroup = 
                Resources.Load<BattleFieldGroup>("ScriptableObjects/BattleFieldGroup/" + dungeon);
            return Instantiate(battleFieldGroup.GetRandomBattleField());
        }

        public void EndBattle()
        {
			
        }
        
    }
}


