using AngelBeat.Core.Battle;
using UnityEngine;
using System.Collections.Generic;
using AngelBeat.Core.SingletonObjects;
using Core.Foundation.Define;

namespace AngelBeat.Core.Map
{
    public class StageLoader : IMapLoader
    {
        private readonly Dictionary<string, StageField> _cache = new();
        private readonly IBattleStageSource _stageSource;
        public StageLoader(IBattleStageSource source)
        {
            _stageSource = source;
        }
        
        public StageField GetBattleField(string stageName=null)
        {
            SystemEnum.eDungeon dungeon = _stageSource.Dungeon;
            
            BattleFieldGroup group = Resources.Load<BattleFieldGroup>($"ScriptableObjects/BattleFieldGroup/{dungeon}");
            if (!group)
            {
                Debug.Log("Group Not Found. Check your enum data");
                return null;
            }

            StageField result;
            if (string.IsNullOrEmpty(stageName))
                result = group.GetRandomBattleField();
            else if(_cache.TryGetValue(stageName, out StageField cachedStage))
                result = cachedStage;
            else
            {
                result = group.GetBattleField(stageName);
                _cache.Add(stageName, result);
            }
            
            if (!result)
            {
                Debug.Log("Stage Not Found. Check your stage name");
                return null;
            }
            return result;
        }
    }
}