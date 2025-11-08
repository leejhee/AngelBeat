using Cysharp.Threading.Tasks;
using GamePlay.Common.Scripts.Entities.Skills;
using GamePlay.Features.Battle.Scripts.Unit;
using System.Threading;
using UnityEngine;
using UnityEngine.Playables;

namespace GamePlay.Common.Scripts.Skill
{
    [RequireComponent(typeof(SkillMarkerReceiver))]
    public class SkillBase : MonoBehaviour
    {
        private SkillModel _model;
        private PlayableDirector _director;
        private bool _isPlaying;
        
        public CharBase CharPlayer { get; private set; }
        public SkillParameter SkillParameter;
        public SkillModel SkillModel => _model;
        
        private void Awake()
        {
            _director = GetComponent<PlayableDirector>();
            if (_director == null)
            {
                Debug.LogError($"{transform.name} PlayableDirector is Null");
            }
        }
        public void SetCharBase(CharBase charBase)
        {
            CharPlayer = charBase;
        }

        public void Init(SkillModel skillModel)
        {
            _model = skillModel;
        }
        
        public void SkillPlay(SkillParameter param)
        {
            // 타임라인 재생
            if (!_director) return;
            SkillParameter = param;
            SkillMarkerReceiver receiver = GetComponent<SkillMarkerReceiver>();
            if (receiver) receiver.Begin(CancellationToken.None);
            
            void OnStopped(PlayableDirector d)
            {
                if (d != _director) return;
                _director.stopped -= OnStopped;
                receiver?.End(); 
            }

            _director.stopped += OnStopped;
            _director.Play();
        }

        public async UniTask PlaySkillAsync(SkillParameter param, CancellationToken ct)
        {
            if (!_director) return;
            if (_isPlaying)
            {
                Debug.LogWarning($"[SkillBase] : {name} Called While Already Playing");
                return;
            }
            _isPlaying = true;
            SkillParameter = param;
            SkillMarkerReceiver receiver = GetComponent<SkillMarkerReceiver>();
            if (receiver) receiver.Begin(ct);
            
            var tcs = new UniTaskCompletionSource();

            void OnStopped(PlayableDirector d)
            {
                if (d != _director) return;
                _director.stopped -= OnStopped;
                receiver?.End();
                tcs.TrySetResult();
            }
            
            _director.stopped += OnStopped;
            try
            {
                _director.time = 0;
                _director.Play();

                using (ct.Register(() =>
                       {
                           try { _director.stopped -= OnStopped; }
                           catch {/* Silence */ }
                           _director.Stop();
                           receiver?.End();
                           tcs.TrySetCanceled(ct);
                       }))
                {
                    if (receiver)
                        await UniTask.WhenAll(tcs.Task, receiver.Completion);
                    else
                        await tcs.Task;
                }
            }
            finally
            {
                SkillParameter = null;
                _isPlaying = false;
            }
        }
        
    }
}
