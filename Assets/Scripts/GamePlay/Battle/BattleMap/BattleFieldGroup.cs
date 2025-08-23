using Core.Foundation.Define;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace AngelBeat.Core.Map
{
    [CreateAssetMenu(fileName = "BattleFieldGroup", menuName = "ScriptableObjects/BattleFieldGroup")]
    public class BattleFieldGroup : ScriptableObject
    {
        [FormerlySerializedAs("eDungeon")] public SystemEnum.Dungeon dungeon;
        public List<StageField> battlefields;

        public StageField GetRandomBattleField()
        {
            if(battlefields == null || battlefields.Count == 0) return null;
            return battlefields[Random.Range(0, battlefields.Count)];
        }

        public StageField GetBattleField(string fieldName)
        {
            if(battlefields == null || battlefields.Count == 0) return null;
            var targetField = battlefields.Find(x => x.name == fieldName);
            if (!targetField)
            {
                Debug.Log($"{fieldName}의 stage를 찾지 못함. 이름이 제대로 되어있는지 확인바람.");
                return null;
            }
            return targetField;
        }
    }
}