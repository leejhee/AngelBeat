using System.Collections.Generic;
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
            CharBase caster = inputParam.Caster;
            CharStat casterStat = caster.CharStat;
            foreach (CharBase target in inputParam.Target)
            {
                CharStat targetStat = target.CharStat;
                
                #region 명중 계산
                float finalAcurracy = 
                    inputParam.Accuracy + 
                    casterStat.GetStat(SystemEnum.eStats.ACCURACY_INCREASE) -
                    targetStat.GetStat(SystemEnum.eStats.DODGE) + 5;
                finalAcurracy = Mathf.Clamp(finalAcurracy, 0, 100);
                bool isHit = Random.Range(0, 100) < finalAcurracy;
                #endregion
                
                #region 대미지 계산
                if (isHit)
                {
                    List<float> stats = new();
                    foreach(SystemEnum.eStats input in inputStats)
                        stats.Add(casterStat.GetStat(input));
                    List<float> keywords = new();
                    foreach (SystemEnum.eKeyword input in inputKeywords)
                        keywords.Add(caster.KeywordInfo.GetKeywordCount(input));
                    float damage = DamageCalculator.Evaluate(damageFormulaInput, stats, keywords);
                    float finalDamage = damage *
                                        inputParam.DamageCalibration *
                                        (casterStat.GetStat(SystemEnum.eStats.DAMAGE_INCREASE) == 0 ? 
                                            1 : casterStat.GetStat(SystemEnum.eStats.DAMAGE_INCREASE)) *
                                        inputParam.CritMultiplier *
                                        (100 - targetStat.GetStat(SystemEnum.eStats.ARMOR)) * 0.01f;
                    targetStat.ReceiveDamage(finalDamage);
                }
                else
                {
                    Debug.Log($"{caster.name}의 공격 {target.name}이 회피");
                }
                #endregion
            }
            
        }

        protected override void SkillInitialize() { }
    }
}