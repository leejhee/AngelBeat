using Core.Data;
using Core.Foundation.Define;
using UnityEngine;

namespace AngelBeat.Core
{
    public class Burn : KeywordBase
    {
        public override SystemEnum.eExecutionPhase Phase => SystemEnum.eExecutionPhase.EoT;

        public Burn(KeywordData data) : base(data)
        {
        }
        
        public override void KeywordExecute(KeywordTriggerContext context)
        {
            Debug.Log($"[키워드 작동] 화상 - 대미지 : {EffectValue}");
            context.Owner.CharStat.ReceiveDamage(EffectValue);
            EffectCount--;
        }
    }
}