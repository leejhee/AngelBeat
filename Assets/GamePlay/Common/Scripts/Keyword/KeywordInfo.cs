using AngelBeat;
using GamePlay.Contracts;
using GamePlay.Features.Scripts.Battle.Unit;
using System.Collections.Generic;
using System.Linq;
using static Core.Scripts.Foundation.Define.SystemEnum;

namespace GamePlay.Features.Scripts.Keyword
{
    public class KeywordInfo
    {
        private readonly Dictionary<eExecutionPhase, List<KeywordBase>> _byPhase = new();
        private readonly ActorID _owner;                  // CharBase → ActorId
        private readonly IKeywordEffectPort _port;        // 도메인 효과는 Port로

        // 필요시: 보유 키워드의 런타임 상태 보관(스택/지속/값)
        private readonly Dictionary<KeywordBase, KeywordRuntime> _rt = new();

        public KeywordInfo(ActorID owner, IKeywordEffectPort port)
        {
            _owner = owner; _port = port;
            for (int i = 1; i < (int)eExecutionPhase.eMax; i++)
                _byPhase.Add((eExecutionPhase)i, new List<KeywordBase>());
        }

        public void AddKeyword(KeywordBase kw, int stacks = 1, int duration = 1, int value = 0)
        {
            _byPhase[kw.Phase].Add(kw);
            _rt[kw] = new KeywordRuntime(stacks, duration, value);
        }

        public bool HasKeyword(eKeyword keyword)
            => _byPhase.Values.SelectMany(l => l).Any(k => k.KeywordType == keyword);

        public void RemoveKeyword(eKeyword keyword)
        {
            foreach (var list in _byPhase.Values)
                list.RemoveAll(k => k.KeywordType == keyword);
            foreach (var k in _rt.Keys.Where(k => k.KeywordType == keyword).ToArray())
                _rt.Remove(k);
        }

        public int GetKeywordCount(eKeyword keyword)
            => _rt.Where(p => p.Key.KeywordType == keyword).Select(p => p.Value.Stacks).FirstOrDefault();

        public int GetKeywordValue(eKeyword keyword)
            => _rt.Where(p => p.Key.KeywordType == keyword).Select(p => p.Value.Value).FirstOrDefault();

        public IEnumerable<KeywordBase> GetKeywordsByPhase(eExecutionPhase phase)
            => _byPhase.TryGetValue(phase, out var list) ? list : Enumerable.Empty<KeywordBase>();

        // 기존 ExecuteByPhase를 Port/ActorId/Runtime과 동작하도록
        public void ExecuteByPhase(eExecutionPhase phase, TriggerType triggerType)
        {
            if (!_byPhase.TryGetValue(phase, out var list)) return;

            for (int i = list.Count - 1; i >= 0; i--)
            {
                var kw = list[i];
                if (!_rt.TryGetValue(kw, out var runtime)) continue;

                var ctx = new KeywordTriggerContext(_owner, triggerType);
                // 키워드가 runtime 변경/효과 호출
                kw.OnTrigger(ctx, ref runtime, _port);

                // 키워드 내부 편의 필드 동기화(선택)
                kw.SyncFrom(ref runtime);

                // 만료 처리(지속/스택 0 이하면 제거)
                if (runtime.Duration <= 0 || runtime.Stacks <= 0)
                {
                    list.RemoveAt(i);
                    _rt.Remove(kw);
                    continue;
                }
                // 변경사항 반영
                _rt[kw] = runtime;
            }
        }
    }    
}
