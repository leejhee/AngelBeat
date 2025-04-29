using System.Collections.Generic;
using UnityEngine;

namespace AngelBeat.Core.Map
{
    [CreateAssetMenu(fileName = "BattleFieldGroup", menuName = "ScriptableObjects/BattleFieldGroup")]
    public class BattleFieldGroup : ScriptableObject
    {
        public SystemEnum.eDungeon dungeon;
        public List<StageField> battlefields;

        public StageField GetRandomBattleField()
        {
            if(battlefields == null || battlefields.Count == 0) return null;
            return battlefields[Random.Range(0, battlefields.Count)];
        }
    }
}