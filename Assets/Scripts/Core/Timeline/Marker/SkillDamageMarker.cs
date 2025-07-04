using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace AngelBeat
{
    
    public class SkillDamageMarker : SkillTimeLineMarker
    {
        [Header("{}은 inputStats의 인덱싱, []은 inputKeywords의 인덱싱")]
        [SerializeField] private string damageFormulaInput;
        [SerializeField] private List<SystemEnum.eStats> inputStats = new();
        [SerializeField] private List<SystemEnum.eKeyword> inputKeywords = new();
        
        public override void MarkerAction()
        {
            
            CharBase caster = InputParam.Caster;

            List<float> statInputs = inputStats.Select(stat => caster.CharStat.GetStat(stat)).ToList();
            List<float> keywordInputs = inputKeywords.Select(kw => (float)caster.KeywordInfo.GetKeywordCount(kw)).ToList();

            float baseDamage = DamageCalculator.Evaluate(damageFormulaInput, statInputs, keywordInputs);

            foreach (CharBase target in InputParam.Target)
            {
                if (target == null) continue;

                DamageParameter param = new()
                {
                    Attacker = caster,
                    Target = target,
                    FinalDamage = baseDamage,
                    SkillType = InputParam.SkillType,
                };

                target.SkillDamage(
                    param,
                    InputParam.Accuracy,
                    InputParam.CritMultiplier * InputParam.DamageCalibration
                    );
            }
            
        }

        protected override void SkillInitialize() { }
        
    }
}