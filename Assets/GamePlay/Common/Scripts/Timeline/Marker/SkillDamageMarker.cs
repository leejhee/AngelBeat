using Core.Scripts.Foundation.Define;
using GamePlay.Common.Scripts.Skill;
using GamePlay.Features.Battle.Scripts;
using GamePlay.Features.Battle.Scripts.Unit;
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

        // 나중에 수정
        [SerializeField] private float Damage;
        [SerializeField] private float CritRate;
        [SerializeField] private float Accuracy;
        public override void MarkerAction()
        {
            CharBase caster = InputParam.Caster;
            float baseDamage = DamageCalculator.Evaluate(damageFormulaInput, inputStats, inputKeywords, caster);
            
            
            foreach (CharBase target in InputParam.Target)
            {
                Debug.Log(target.CharInfo.Name);
                
                if (target == null) continue;

                DamageParameter param = new()
                {
                    Attacker = caster,
                    Target = target,
                    FinalDamage = Damage,
                    SkillType = InputParam.SkillType,
                };

                // TODO: accuracy 수정할것
                target.SkillDamage(
                    param,
                    100,
                    InputParam.CritMultiplier //* InputParam.DamageCalibration
                    );
            }
            
        }

        protected override void SkillInitialize() { }
        
    }
}