using Cysharp.Threading.Tasks;
using GamePlay.Features.Battle.Scripts;
using GamePlay.Features.Battle.Scripts.BattleAction;
using GamePlay.Features.Battle.Scripts.Unit;
using System;
using System.Collections;
using System.Threading;
using UnityEngine;

namespace GamePlay.Common.Scripts.Timeline.Marker
{
    public class SkillPushTargetMarker : SkillTimeLineMarker
    {
        private CharBase _target;
        
        public override async UniTask BuildTaskAsync(CancellationToken ct)
        {
            _target = InputParam.Target[0]; // 단일 타겟이라 가정
            var grid = InputParam.Grid;
            Vector2Int pivot = grid.WorldToCell(InputParam.Caster.CharTransform.position);
            Vector2Int targetCell = grid.WorldToCell(_target.CharTransform.position);
            var res = PushEngine.ComputePushResult(pivot, targetCell, grid);
            await PushEngine.ApplyPushResult(_target, res, grid, ct, true);
        }
    }
}