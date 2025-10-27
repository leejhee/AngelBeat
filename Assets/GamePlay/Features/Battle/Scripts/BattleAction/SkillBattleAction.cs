using Core.Scripts.Data;
using Cysharp.Threading.Tasks;
using GamePlay.Common.Scripts.Entities.Skills;
using GamePlay.Common.Scripts.Skill;
using GamePlay.Features.Battle.Scripts.BattleMap;
using GamePlay.Features.Battle.Scripts.Unit;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace GamePlay.Features.Battle.Scripts.BattleAction
{
    public class SkillBattleAction : BattleActionBase
    {
        public SkillBattleAction(BattleActionContext ctx) : base(ctx)
        { }

        public override UniTask<BattleActionPreviewData> BuildActionPreview(CancellationToken ct)
        {
            if(Context == null) throw new InvalidOperationException("[SkillBattleAction] - Context is null");
            
            SkillModel model = Context.skillModel;
            if(model == null) throw new InvalidOperationException("[SkillBattleAction] - SkillModel is null");
            SkillRangeData range = model.skillRange;
            
            CharBase actor = Context.actor;
            BattleStageGrid stageGrid = Context.battleField.GetComponent<BattleStageGrid>();
            List<Vector2Int> possible = new();
            List<Vector2Int> blocked = new();
            Vector2Int pivot = stageGrid.WorldToCell(actor.CharTransform.position);
            
            #region Calculation
            
            #region 0, 0 계산
            if (range.Origin)
            {
                if(stageGrid.IsInBounds(pivot) && stageGrid.IsWalkable(pivot)) possible.Add(pivot);
                else blocked.Add(pivot);
            }
            #endregion
            
            bool hit = false;
            #region y = 0 계산
            for (int i = 1; i <= range.Forward; i++)
            {
                var c = new Vector2Int(pivot.x + i, pivot.y);
                if (!stageGrid.IsInBounds(c) || !stageGrid.IsWalkable(c)) hit = true;
                (hit ? blocked : possible).Add(c);
                if (hit) break;
            }
            hit = false;
            for (int i = 1; i <= range.Backward; i++)
            {
                var c = new Vector2Int(pivot.x - i, pivot.y);
                if (!stageGrid.IsInBounds(c) || !stageGrid.IsWalkable(c)) hit = true;
                (hit ? blocked : possible).Add(c);
                if (hit) break;
            }
            #endregion
            
            #region y = 1 계산
            if (range.Up)
            {
                var c = new Vector2Int(pivot.x, pivot.y + 1);
                (stageGrid.IsInBounds(c) && stageGrid.IsWalkable(c) ? possible : blocked).Add(c);
            }
            int upY = pivot.y + 1;
            hit = false;
            for (int i = 1; i <= range.UpForward; i++)
            {
                var c = new Vector2Int(pivot.x + i, upY);
                if (!stageGrid.IsInBounds(c) || !stageGrid.IsWalkable(c)) hit = true;
                (hit ? blocked : possible).Add(c);
                if (hit) break;
            }
            hit = false;
            for (int i = 1; i <= range.UpBackward; i++)
            {
                var c = new Vector2Int(pivot.x - i, upY);
                if (!stageGrid.IsInBounds(c) || !stageGrid.IsWalkable(c)) hit = true;
                (hit ? blocked : possible).Add(c);
                if (hit) break;
            }
            
            #endregion
            
            #region y = -1 계산
            if (range.Down)
            {
                var c = new Vector2Int(pivot.x, pivot.y - 1);
                (stageGrid.IsInBounds(c) && stageGrid.IsWalkable(c) ? possible : blocked).Add(c);
            }
            int downY = pivot.y - 1;
            hit = false;
            for (int i = 1; i <= range.DownForward; i++)
            {
                var c = new Vector2Int(pivot.x + i, downY);
                if (!stageGrid.IsInBounds(c) || !stageGrid.IsWalkable(c)) hit = true;
                (hit ? blocked : possible).Add(c);
                if (hit) break;
            }
            hit = false;
            for (int i = 1; i <= range.DownBackward; i++)
            {
                var c = new Vector2Int(pivot.x - i, downY);
                if (!stageGrid.IsInBounds(c) || !stageGrid.IsWalkable(c)) hit = true;
                (hit ? blocked : possible).Add(c);
                if (hit) break;
            }
            #endregion
            #endregion

            return UniTask.FromResult(new BattleActionPreviewData(possible, blocked));
        }

        public override async UniTask<BattleActionResult> ExecuteAction(CancellationToken ct)
        {
            if (Context.TargetCell == null || Context.actor == null || Context.battleField == null)
                return BattleActionResult.Fail(BattleActionResult.ResultReason.InvalidTarget);
            if(Context.skillModel == null)
                return BattleActionResult.Fail(BattleActionResult.ResultReason.InvalidContext);

            CharBase caster = Context.actor;
            //StageField stage = Context.battleField;
            //Vector2Int cell = Context.TargetCell.Value;
            //Vector2 targetWorld = stage.CellToWorldCenter(cell);
            List<CharBase> targets = Context.targets ?? new List<CharBase>();

            SkillParameter parameter = new(
                caster: caster,
                target: targets,
                model: Context.skillModel
            );
            
            await caster.SkillInfo.PlaySkillAsync(Context.skillModel.SkillIndex, parameter, ct);
            
            return BattleActionResult.Success();
        }
        
    }
}