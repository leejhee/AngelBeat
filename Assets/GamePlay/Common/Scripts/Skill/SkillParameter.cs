using Core.Scripts.Foundation.Define;
using GamePlay.Common.Scripts.Entities.Skills;
using GamePlay.Features.Battle.Scripts.BattleMap;
using GamePlay.Features.Battle.Scripts.Unit;
using System.Collections.Generic;

namespace GamePlay.Common.Scripts.Skill
{
    public class SkillParameter
    {
        public readonly CharBase                Caster;
        public readonly List<CharBase>          Target;
        public readonly SkillModel              Model;
        public readonly SystemEnum.eSkillType   SkillType;
        public readonly BattleStageGrid         Grid;
        
        public SkillParameter(
            CharBase caster, 
            List<CharBase> target,
            SkillModel model,
            BattleStageGrid grid
        )
        {
            Caster = caster;
            Target = target;
            Model = model;
            Grid = grid;
            SkillType = model.skillType;
        }
    }
}