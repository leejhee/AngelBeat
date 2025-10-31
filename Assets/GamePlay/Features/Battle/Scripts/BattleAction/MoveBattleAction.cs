using AngelBeat;
using Core.Scripts.Foundation.Define;
using Cysharp.Threading.Tasks;
using GamePlay.Common.Scripts.Entities.Character.Components;
using GamePlay.Features.Battle.Scripts.BattleMap;
using GamePlay.Features.Battle.Scripts.Unit;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace GamePlay.Features.Battle.Scripts.BattleAction
{
    public class MoveBattleAction : BattleActionBase
    {
        public MoveBattleAction(BattleActionContext ctx) : base(ctx)
        { }

        public override UniTask<BattleActionPreviewData> BuildActionPreview(CancellationToken ct)
        {
            if(Context == null) throw new InvalidOperationException("Context is null");

            // 이동 포인트를 가져오기
            CharBase actor = Context.actor;
            CharStat stat = actor.RuntimeStat;
            long movePoint = stat.GetStat(SystemEnum.eStats.NACTION_POINT);
            
            // 갈수없는곳 나올때까지 좌우로 탐색하여 반환 구조체에 담기
            
            List<Vector2Int> movablePoints = new();
            List<Vector2Int> blockedPoints = new();
            
            BattleStageGrid stageGrid = Context.battleField.GetComponent<BattleStageGrid>();
            Vector2Int pivot = stageGrid.WorldToCell(actor.CharTransform.position);
            
            #region Right Inspection

            bool blocked = false;
            for (int offset = 1; offset <= (int)movePoint; offset++)
            {
                Vector2Int candidate = new Vector2Int(pivot.x + offset, pivot.y);
                if (stageGrid.IsMaskable(candidate)) continue;
                if (stageGrid.IsWalkable(candidate))
                {
                    movablePoints.Add(candidate);
                }
                else if(blocked)
                {
                    blockedPoints.Add(candidate);
                }
                else
                {
                    blocked = true;
                    blockedPoints.Add(candidate);
                }
            }
            #endregion
            #region Left Inspection
            for (int offset = 1; offset <= (int)movePoint; offset++)
            {
                Vector2Int candidate = new Vector2Int(pivot.x - offset, pivot.y);
                if (stageGrid.IsMaskable(candidate)) continue;
                if (stageGrid.IsWalkable(candidate))
                {
                    movablePoints.Add(candidate);
                }
                else if(blocked)
                {
                    blockedPoints.Add(candidate);
                }
                else
                {
                    blocked = true;
                    blockedPoints.Add(candidate);
                }
            }
            #endregion
            
            // 상태 머신 그냥 안만들기 위해서
            return UniTask.FromResult(new BattleActionPreviewData(movablePoints, blockedPoints));
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
            Vector2Int pivot = grid.WorldToCell(actor.CharTransform.position);
            
            // 캐릭터 프리팹 이동
            var toWorld = stage.CellToWorldCenter(goal);
            long delta = -Mathf.Abs(goal.x - pivot.x);
            actor.RuntimeStat.ChangeStat(SystemEnum.eStats.NACTION_POINT, delta);
            await actor.CharMove(toWorld);
            
            // 그리드 위치정보 저장
            grid.MoveUnit(actor, goal);
            return BattleActionResult.Success();
        }
    }
}