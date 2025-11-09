using Core.Scripts.Data;
using Core.Scripts.Managers;
using GamePlay.Features.Battle.Scripts.Unit;
using GamePlay.Features.Scripts.Keyword;
using System.Collections.Generic;
using static Core.Scripts.Foundation.Define.SystemEnum;

namespace GamePlay.Common.Scripts.Keyword
{
    public class KeywordInfo
    {
        private readonly Dictionary<eExecutionPhase, List<KeywordBase>> _byPhase = new();

        private readonly Dictionary<eKeyword, KeywordBase> _byType = new();
        
        
        public KeywordInfo(CharBase owner)
        {
            for (int i = 1; i < (int)eExecutionPhase.eMax; i++)
                _byPhase.Add((eExecutionPhase)i, new List<KeywordBase>());
        }

        public void AddKeyword(eKeyword kw, int stacks = 1, int duration = 1, int value = 0)
        {
            KeywordData _data = DataManager.Instance.KeywordMap[kw];
            KeywordBase keyword = KeywordFactory.CreateKeyword(_data);
            keyword.EffectCount = duration;
            keyword.EffectValue = stacks;
            _byType.TryAdd(_data.keywordType, keyword);
            
            //_byPhase[_data.].Add(kw);
            //_byType[kw] = new KeywordRuntime(stacks, duration, value);
        }
//
        //public bool HasKeyword(eKeyword keyword)
        //    => _byPhase.Values.SelectMany(l => l).Any(k => k.KeywordType == keyword);
//
        //public void RemoveKeyword(eKeyword keyword)
        //{
        //    foreach (var list in _byPhase.Values)
        //        list.RemoveAll(k => k.KeywordType == keyword);
        //    foreach (var k in _rt.Keys.Where(k => k.KeywordType == keyword).ToArray())
        //        _rt.Remove(k);
        //}
//
        public int GetKeywordCount(eKeyword keyword)
        {
            if (keyword == eKeyword.None) return 0;
            if (!_byType.ContainsKey(keyword)) return 0;
            return _byType[keyword].EffectCount;
        }
        
        public int GetKeywordValue(eKeyword keyword)
        {
            if (keyword == eKeyword.None) return 0;
            if (!_byType.ContainsKey(keyword)) return 0;
            return _byType[keyword].EffectValue;
        }
        
    }    
}
