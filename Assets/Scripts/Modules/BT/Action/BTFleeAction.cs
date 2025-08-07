﻿using AngelBeat;
using AngelBeat.Core.SingletonObjects.Managers;
using Character.Unit;
using Modules.BT.Nodes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Modules.BT.Action
{
    public class BTFleeAction : IBTAction
    {
        private List<CharBase> _enemies;
        
        public BTNode.State Execute(BTContext context)
        {
            _enemies = BattleCharManager.Instance.GetEnemies(context.Agent.GetCharType());
            return BTNode.State.Success;
        }
    }
}