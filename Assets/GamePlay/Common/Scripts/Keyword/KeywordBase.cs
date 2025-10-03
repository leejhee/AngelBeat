using Core.Scripts.Data;
using Core.Scripts.Foundation.Define;
using GamePlay.Contracts;

namespace GamePlay.Features.Scripts.Keyword
{
    public abstract class KeywordBase
    {
        protected readonly KeywordData _data;          // 원래 데이터 유지 가능
        public abstract SystemEnum.eExecutionPhase Phase { get; } // 기존 그대로 (턴/타이밍 구분)
        public SystemEnum.eKeyword KeywordType => _data.keywordType;

        protected KeywordBase(KeywordData data) { _data = data; }

        // 핵심: 도메인 행동은 Port로, 잔여/스택은 runtime로
        //public abstract void OnTrigger(in KeywordTriggerContext ctx, ref KeywordRuntime runtime, IKeywordEffectPort port);

        public int EffectValue { get; set; }
        public int EffectCount { get; set; }
        //protected internal void SyncFrom(ref KeywordRuntime rt) { EffectCount = rt.Duration; EffectValue = rt.Value; }
        //protected internal void SyncTo(ref KeywordRuntime rt)   { rt.Duration = EffectCount; rt.Value = EffectValue; }

    }
}