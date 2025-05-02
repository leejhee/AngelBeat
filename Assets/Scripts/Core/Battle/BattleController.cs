using AngelBeat.Core.Character.Party;
using AngleBeat.Core.SingletonObjects;
using System;
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
            Party party = BattlePayload.Instance.PlayerParty;
            SystemEnum.eDungeon dungeon = BattlePayload.Instance.DungeonName;
            
           
        }

        public void InitializeBattle()
        {
            InitEnvironment();
        }

        private void InitEnvironment()
        {
            //일단 맵을 로드해야한다.
        }

        public void EndBattle()
        {
			
        }
        
    }
}


