using Core.Scripts.Data;
using Core.Scripts.Foundation.Define;
using GamePlay.Contracts;
using UnityEngine;

namespace GamePlay.Features.Scripts.Keyword
{
    public class Burn : KeywordBase
    {
        public override SystemEnum.eExecutionPhase Phase => SystemEnum.eExecutionPhase.EoT;
        public override void OnTrigger(in KeywordTriggerContext ctx, ref KeywordRuntime runtime, IKeywordEffectPort port)
        {
            throw new System.NotImplementedException();
        }

        public Burn(KeywordData data) : base(data)
        {
        }
        
        //public override void KeywordExecute(KeywordTriggerContext context)
        //{
        //    Debug.Log($"[키워드 작동] 화상 - 대미지 : {EffectValue}");
        //    context.Owner.CharStat.ReceiveDamage(EffectValue);
        //    EffectCount--;
        //}
    }
}