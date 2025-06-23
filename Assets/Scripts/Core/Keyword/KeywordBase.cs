using static SystemEnum;
using AngelBeat;

namespace AngelBeat
{
    public abstract class KeywordBase
    {
        private KeywordData data;
        private string name;
        public int EffectValue { get; protected set; }
        public int EffectCount { get; protected set; }
        public abstract eExecutionPhase Phase { get;}
        public eKeyword KeywordType => data.keywordType;

        protected KeywordBase(KeywordData data)
        {
            this.data = data;
        }

        public void ChangeEffect(int valueDelta, int countDelta)
        {
            EffectValue += valueDelta;
            EffectCount += countDelta;
        }

        public abstract void KeywordExecute(KeywordTriggerContext context);
    }
}
