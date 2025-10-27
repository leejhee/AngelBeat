using Core.Scripts.Foundation.Define;
using GamePlay.Common.Scripts.Entities.Skills;
using GamePlay.Features.Battle.Scripts.Unit;
using System.Collections.Generic;

namespace GamePlay.Common.Scripts.Skill
{
    public class SkillParameter
    {
        public readonly CharBase Caster;
        public readonly List<CharBase> Target;
        public readonly SkillModel Model;
        public readonly SystemEnum.eSkillType SkillType;
        public readonly float Accuracy;              
        public readonly float CritMultiplier;
        
        public SkillParameter(
            CharBase caster, 
            List<CharBase> target,
            SkillModel model
        )
        {
            Caster = caster;
            Target = target;
            Model = model;
            SkillType = model.skillType;
            Accuracy = model.skillAccuracy;
            CritMultiplier = model.critCalibration;
        }
    }
}