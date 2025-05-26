using static SystemEnum;
using AngelBeat;

namespace AngelBeat.Core
{
    public abstract class KeywordBase
    {
        private KeywordData data;
        private string name;
        public int EffectValue { get; private set; }
        public int EffectCount { get; private set; }
        public eExecutionPhase Phase { get; private set; }

        protected KeywordBase(KeywordData data)
        {
            this.data = data;
        }

        public void ChangeEffect(int valueDelta, int countDelta)
        {
            EffectValue += valueDelta;
            EffectCount += countDelta;
            
            // 바뀌는 키워드도 있을 수 있어야 한다.
            // EventBus.Instance.SendMessage(new OnKeywordChange)
        }

        public abstract void KeywordExecute();
    }

    public class 화상 : KeywordBase
    {
        public 화상(KeywordData data) : base(data){}

        public override void KeywordExecute()
        {
            // 현재 있는 양만큼 대미지를 입어야 한다.
            
        }
    }
}
