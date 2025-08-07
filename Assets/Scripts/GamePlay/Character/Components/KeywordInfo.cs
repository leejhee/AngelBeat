using Character.Unit;
using System.Collections.Generic;
using System.Linq;
using static SystemEnum;

namespace AngelBeat
{
    // Linq를 왜 써요...? 
    // Update 단위로 호출하지 않으므로 쓴다.
    public class KeywordInfo
    {
        private Dictionary<eExecutionPhase, List<KeywordBase>>   _executionPhaseDict = new();
        private CharBase _keywordOwner;
        
        public KeywordInfo(CharBase owner)
        {
            _keywordOwner = owner;
            for (int i = 1; i < (int)eExecutionPhase.eMax; i++)
            {
                _executionPhaseDict.Add((eExecutionPhase)i, new List<KeywordBase>());
            }
        }
        
        public void AddKeyword(KeywordBase keywordBase)
        {
            var key = keywordBase.Phase;
            _executionPhaseDict[key].Add(keywordBase);
        }

        public void AddKeyword(eKeyword keyword)
        {
            
        }
        
        public bool HasKeyword(eKeyword keyword)
        {
            return _executionPhaseDict.Values
                .SelectMany(list => list)
                .Any(k => k.KeywordType == keyword);
        }

        public void RemoveKeyword(eKeyword keyword)
        {
            foreach (var list in _executionPhaseDict.Values)
            {
                list.RemoveAll(k => k.KeywordType == keyword);
            }
        }

        public int GetKeywordCount(eKeyword keyword)
        {
            var kw = _executionPhaseDict.Values
                .SelectMany(list => list)
                .FirstOrDefault(k => k.KeywordType == keyword);
            return kw?.EffectCount ?? 0;
        }

        public int GetKeywordValue(eKeyword keyword)
        {
            var kw = _executionPhaseDict.Values
                .SelectMany(list => list)
                .FirstOrDefault(k => k.KeywordType == keyword);
            return kw?.EffectValue ?? 0;
        }

        public IEnumerable<KeywordBase> GetKeywordsByPhase(eExecutionPhase phase)
        {
            return _executionPhaseDict.TryGetValue(phase, out var list)
                ? list
                : Enumerable.Empty<KeywordBase>();
        }

        public void KeywordChange(OnKeywordChange onKeywordChange)
        {
            // TODO: 외부로 변화 통지하거나 UI 연동 시 여기 확장
        }
        
        public void ExecuteByPhase(eExecutionPhase phase)
        {
            if (!_executionPhaseDict.TryGetValue(phase, out List<KeywordBase> list)) return;

            for (int i = list.Count - 1; i >= 0; i--)
            {
                var keyword = list[i];
                var context = new KeywordTriggerContext
                {
                    Owner = _keywordOwner
                };

                keyword.KeywordExecute(context);

                if (keyword.EffectCount <= 0)
                {
                    list.RemoveAt(i);
                    // onKeywordChange 이벤트 후처리 필요 시 여기에
                }
            }
        }
    }    
}
