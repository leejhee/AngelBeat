using System;
using System.Collections.Generic;
using UnityEngine;

namespace AngelBeat.Core.Battle
{
    public class BattleController : MonoBehaviour
    {
        private List<CharBase> _battleCharList;
        private TurnController _turnManager;
        
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


