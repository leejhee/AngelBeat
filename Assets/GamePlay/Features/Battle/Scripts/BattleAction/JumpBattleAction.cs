using Cysharp.Threading.Tasks;
using GamePlay.Features.Battle.Scripts.BattleMap;
using GamePlay.Features.Battle.Scripts.Unit;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace GamePlay.Features.Battle.Scripts.BattleAction
{
    public class JumpBattleAction : BattleAction
    {
        private static readonly List<Vector2Int> JumpableRange =  new()
        {
            new Vector2Int(-1, -1),
            new Vector2Int(0, -1),
            new Vector2Int(1, -1),
            new Vector2Int(2, 0),
            new Vector2Int(-2, 0),
            new Vector2Int(-1, 1),
            new Vector2Int(0, 1),
            new Vector2Int(1, 1)
        };

        public JumpBattleAction(BattleActionContext ctx) : base(ctx)
        {}

        public override UniTask<BattleActionPreviewData> BuildActionPreview(CancellationToken ct)
        {
            if(Context == null) throw new InvalidOperationException("[JumpBattleAction] - Context is null");
            
            CharBase actor = Context.actor;
            BattleStageGrid stageGrid = Context.battleField.GetComponent<BattleStageGrid>();
            List<Vector2Int> jumpablePoints = new();
            List<Vector2Int> blockedPoints = new();
            Vector2Int pivot = stageGrid.WorldToCell(actor.CharTransform.position);
                
            foreach (Vector2Int candidate in JumpableRange)
            {
                Vector2Int sum = pivot + candidate;
                if (stageGrid.IsWalkable(sum))
                {
                    jumpablePoints.Add(sum);
                }
                else
                {
                    blockedPoints.Add(sum);
                }
            }
            
            return UniTask.FromResult(new BattleActionPreviewData(jumpablePoints, blockedPoints));
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
            Vector2Int goal = Context.TargetCell.Value;
            
            var toWorld = stage.CellToWorldCenter(goal);
            await actor.CharJump(toWorld, ct);
            
            grid.MoveUnit(actor, goal);
            return BattleActionResult.Success();
        }
    }
}