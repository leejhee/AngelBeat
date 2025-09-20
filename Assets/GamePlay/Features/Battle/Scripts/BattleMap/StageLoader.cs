using Core.Scripts.Managers;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

namespace GamePlay.Features.Battle.Scripts.BattleMap
{
    public class StageLoader : IMapLoader
    {
        private readonly IBattleStageSource _stageSource;
        private readonly BattleFieldDB _db;
        
        public StageLoader(IBattleStageSource source, BattleFieldDB db)
        {
            _stageSource = source;
            _db = db;
        }

        /// <summary>
        /// Load에서 바로 instantiate로 변경 : Load만 하고 들고만 있진 않을 것 같아서.
        /// </summary>
        public async UniTask<StageField> InstantiateBattleFieldAsync(
            string stageName = null, Transform parent = null, CancellationToken ct = default)
        {
            BattleFieldGroup group = _db.Resolve(_stageSource.Dungeon);
            if (!group)
            {
                throw new System.Exception($"BattleFieldGroup not found for {_stageSource.Dungeon}");
            }

            var stageRef = group.GetStageRef(stageName);
            if (stageRef == null) throw new Exception($"Stage not found : {stageName}");

            var go = await ResourceManager.Instance.InstantiateAsync(stageRef, parent, false, ct);
            var field = go.GetComponent<StageField>();
            if (!field) throw new Exception($"Stage Component Missing : {stageName}");
            return field;

        }
    }
}