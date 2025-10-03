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
        
        
        
        //public IEnumerable<KeywordBase> GetKeywordsByPhase(eExecutionPhase phase)
        //    => _byPhase.TryGetValue(phase, out var list) ? list : Enumerable.Empty<KeywordBase>();
//
        //// 기존 ExecuteByPhase를 Port/ActorId/Runtime과 동작하도록
        //public void ExecuteByPhase(eExecutionPhase phase, TriggerType triggerType)
        //{
        //    if (!_byPhase.TryGetValue(phase, out var list)) return;
//
        //    for (int i = list.Count - 1; i >= 0; i--)
        //    {
        //        var kw = list[i];
        //        if (!_rt.TryGetValue(kw, out var runtime)) continue;
//
        //        var ctx = new KeywordTriggerContext(_owner, triggerType);
        //        // 키워드가 runtime 변경/효과 호출
        //        kw.OnTrigger(ctx, ref runtime, _port);
//
        //        // 키워드 내부 편의 필드 동기화(선택)
        //        kw.SyncFrom(ref runtime);
//
        //        // 만료 처리(지속/스택 0 이하면 제거)
        //        if (runtime.Duration <= 0 || runtime.Stacks <= 0)
        //        {
        //            list.RemoveAt(i);
        //            _rt.Remove(kw);
        //            continue;
        //        }
        //        // 변경사항 반영
        //        _rt[kw] = runtime;
        //    }
        //}
    }    
}
