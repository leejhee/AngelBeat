using Core.Scripts.Data;
using Cysharp.Threading.Tasks;
using GamePlay.Common.Scripts.Entities.Skills;
using GamePlay.Common.Scripts.Skill;
using GamePlay.Features.Battle.Scripts.BattleMap;
using GamePlay.Features.Battle.Scripts.Unit;
using System;
using System.Collections.Generic;
using System.Threading;

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
            BattleActionPreviewData data = SkillRangeHelper.ComputeSkillRange(stageGrid, range, actor);
            
            return UniTask.FromResult(data);
        }

        public override async UniTask<BattleActionResult> ExecuteAction(CancellationToken ct)
        {
            if (Context.TargetCell == null || Context.actor == null || Context.battleField == null)
                return BattleActionResult.Fail(BattleActionResult.ResultReason.InvalidTarget);
            if(Context.skillModel == null)
                return BattleActionResult.Fail(BattleActionResult.ResultReason.InvalidContext);

            CharBase caster = Context.actor;
            List<CharBase> targets = Context.targets ?? new List<CharBase>();

            SkillParameter parameter = new(
                caster: caster,
                target: targets,
                grid : Context.battleField.GetComponent<BattleStageGrid>()
            );
            
            await caster.SkillInfo.PlaySkillAsync(Context.skillModel.SkillIndex, parameter, ct);
            
            return BattleActionResult.Success();
        }
        
    }
}