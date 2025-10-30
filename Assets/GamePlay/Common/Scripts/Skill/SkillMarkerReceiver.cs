using AngelBeat;
using Cysharp.Threading.Tasks;
using GamePlay.Common.Scripts.Timeline.Marker;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Playables;

namespace GamePlay.Common.Scripts.Skill
{
    public class SkillMarkerReceiver : MonoBehaviour, INotificationReceiver
    {
        private SkillParameter _input;
        private List<UniTask> _pending = new();
        private UniTaskCompletionSource _allDone;
        private CancellationToken _ct;
        
        public SkillParameter Input => _input;
        public UniTask Completion => _allDone?.Task ?? UniTask.CompletedTask;

        public void Begin(SkillParameter input, CancellationToken ct)
        {
            _input = input;
            _ct = ct;
            _pending.Clear();
            _allDone = new UniTaskCompletionSource();
        }

        private async UniTaskVoid CompleteAsync()
        {
            try
            {
                if (_pending.Count > 0)
                    await UniTask.WhenAll(_pending);
            }
            finally
            {
                _pending.Clear();
                _allDone?.TrySetResult();
            }
        }
        
        public void End()
        {
            CompleteAsync().Forget();
        }
        
        private void Track(UniTask task) => _pending.Add(task.SuppressCancellationThrow());
        
        public void OnNotify(Playable origin, INotification notification, object context)
        {
            if (notification is SkillTimeLineMarker skillMarker)
            {
                skillMarker.InitInput(Input);
                skillMarker.AttachTracker(Track);

                var task = skillMarker.BuildTaskAsync(_ct);
                Track(task);
                
                //skillMarker.MarkerAction();
            }
        }
    }
}