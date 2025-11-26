using Cysharp.Threading.Tasks;
using GamePlay.Features.Battle.Scripts.BattleMap;
using GamePlay.Features.Battle.Scripts.Unit;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace GamePlay.Features.Battle.Scripts.BattleAction
{
    public class JumpBattleAction : BattleActionBase
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
            
            BattleActionPreviewData resultData = BattleRangeHelper.ComputeJumpRangeFromClient(stageGrid, actor);
            
            return UniTask.FromResult(resultData);
        }

        public override async UniTask<BattleActionResult> ExecuteAction(CancellationToken ct)
        {
            if(Context == null || !Context.actor || !Context.battleField)
                return BattleActionResult.Fail(BattleActionResult.ResultReason.InvalidContext);
            if(Context.TargetCell == null)
                return BattleActionResult.Fail(BattleActionResult.ResultReason.InvalidTarget);
            
            StageField stage = Context.battleField;
            BattleStageGrid grid = stage.GetComponent<BattleStageGrid>();
            CharBase actor = Context.actor;
            Vector2Int goal = Context.TargetCell.Value;
            
            //var toWorld = stage.CellToWorldCenter(goal);
            //float yCalibration = stage.Grid.cellSize.y / 2;
            //float yCollider = actor.BattleCollider.size.y / 2;
            //(new Vector2(toWorld.x, toWorld.y - (yCalibration - yCollider)));
            var finalPos = grid.CalibratedPivot(goal, actor); 
            //await actor.CharJump(finalPos, ct);
            var driver = BattleController.Instance.CameraDriver;
            var jumpTask = actor.CharJump(finalPos, ct);
            if (driver)
            {
                await driver.FollowDuringAsync(actor.CharCameraPos, jumpTask, 0.25f, driver.FocusOrthoSize, 0.25f);
            }
            else
            {
                await jumpTask;
            }

            grid.MoveUnit(actor, goal);
            return BattleActionResult.Success();
        }
    }
}