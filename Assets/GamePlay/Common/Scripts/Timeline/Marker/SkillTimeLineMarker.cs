using Cysharp.Threading.Tasks;
using GamePlay.Common.Scripts.Skill;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace GamePlay.Common.Scripts.Timeline.Marker
{
    public abstract class SkillTimeLineMarker : UnityEngine.Timeline.Marker, INotification
    {
        public PropertyName id => new PropertyName("SkillTimeLineMarker");

        private Action<UniTask> _trackHook;
        protected SkillParameter InputParam;
        
        public void AttachTracker(Action<UniTask> trackHook) => _trackHook = trackHook;
        protected void Track(UniTask task) => _trackHook?.Invoke(task);
        
        public virtual void InitInput(SkillParameter input) => InputParam = input;

        public virtual void MarkerAction() { }

        public virtual UniTask BuildTaskAsync(CancellationToken ct)
        {
            try { MarkerAction(); }
            catch (System.Exception e) { Debug.LogException(e); }
            return UniTask.CompletedTask;
        }
        
        protected virtual void SkillInitialize() { }

        public override void OnInitialize(TrackAsset aPent)
        {
            base.OnInitialize(aPent);
            SkillInitialize();
        }
        
    }
    
    

}