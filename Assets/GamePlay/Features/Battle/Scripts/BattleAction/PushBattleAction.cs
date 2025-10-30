using AngelBeat;
using Core.Scripts.Foundation.Define;
using Cysharp.Threading.Tasks;
using GamePlay.Features.Battle.Scripts.BattleMap;
using GamePlay.Features.Battle.Scripts.Unit;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace GamePlay.Features.Battle.Scripts.BattleAction
{
    public class PushBattleAction : BattleActionBase
    {
        // 앞뒤만 밀 수 있다
        private static readonly List<Vector2Int> PushableRange = new() { new Vector2Int(-1, 0), new Vector2Int(1, 0) };
        
        public PushBattleAction(BattleActionContext ctx) : base(ctx)
        { }
        
        public override UniTask<BattleActionPreviewData> BuildActionPreview(CancellationToken ct)
        {
            if(Context == null) throw new InvalidOperationException("Context is null");

            CharBase actor = Context.actor;
            
            List<Vector2Int> pushablePoints = new();
            List<Vector2Int> blockedPoints = new();
            
            BattleStageGrid stageGrid = Context.battleField.GetComponent<BattleStageGrid>();
            Vector2Int pivot = stageGrid.WorldToCell(actor.CharTransform.position);

            foreach (Vector2Int point in PushableRange)
            {
                Vector2Int candidate = pivot + point;
                if (stageGrid.IsMaskable(candidate)) continue;
                if (stageGrid.IsOccupied(candidate) && stageGrid.IsPlatform(candidate))
                {
                    pushablePoints.Add(candidate);
                }
                else
                {
                    blockedPoints.Add(candidate);
                }
            }
            // 상태 머신 그냥 안만들기 위해서
            return UniTask.FromResult(new BattleActionPreviewData(pushablePoints, blockedPoints));
        }

        public override async UniTask<BattleActionResult> ExecuteAction(CancellationToken ct)
        {
            if(Context == null || Context.actor == null || Context.battleField == null)
                return BattleActionResult.Fail(BattleActionResult.ResultReason.InvalidContext);
            if(Context.TargetCell == null)
                return BattleActionResult.Fail(BattleActionResult.ResultReason.InvalidTarget);

            StageField stage = Context.battleField;
            BattleStageGrid grid = stage.GetComponent<BattleStageGrid>();
            
            CharBase actor = Context.actor;
            Vector2Int pivot = grid.WorldToCell(actor.CharTransform.position);
            
            Vector2Int targetingCell = Context.TargetCell.Value;
            CharBase victim = grid.GetUnitAt(targetingCell);

            PushEngine.PushResult res = PushEngine.ComputePushResult(pivot, targetingCell, grid);
            actor.CharPushPlay();
            await PushEngine.ApplyPushResult(victim, res, grid, ct);
            return BattleActionResult.Success();
            
            //Vector2Int goal = targetingCell + (targetingCell - pivot);
            //Vector2 toWorld = stage.CellToWorldCenter(goal);

            //Vector2 toWorld = grid.CellToWorldCenter(firstGoal);
            //actor.CharPushPlay();
            // 캐릭터 프리팹 이동
            //await victim.CharKnockBack(toWorld); 
            // TODO : 벽꿍 구현할 것

            // 다 움직이고 나서 만약 떨어질 경우 : 낙차만큼 피해 및 사망 예약
            //if (!grid.IsPlatform(goal))
            //{
            //    if (!grid.TryFindLandingFloor(goal, out Vector2Int landing, out int fallCells))
            //    {
            //        await victim.CharKnockBack(grid.CellToWorldCenter(new Vector2Int(goal.x, -1)));
            //        grid.RemoveUnit(victim);
            //        victim.CharDead();
            //        
            //        return BattleActionResult.Success();
            //    }
//
            //    await victim.CharKnockBack(grid.CellToWorldCenter(landing));
            //    grid.MoveUnit(victim, landing);
            //    victim.RuntimeStat.ReceiveHPPercentDamage(30 * fallCells);
            //    return BattleActionResult.Success();
            //}
            //
            //grid.MoveUnit(victim, goal);
            //return BattleActionResult.Success();
        }
    }
}