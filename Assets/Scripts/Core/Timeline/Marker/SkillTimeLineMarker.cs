using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Timeline;
using UnityEngine.Playables;

namespace AngelBeat
{
    public abstract class SkillTimeLineMarker : Marker, INotification
    {
        public PropertyName id => new PropertyName("SkillTimeLineMarker");
        protected SkillParameter InputParam;

        public abstract void MarkerAction();
        protected virtual void SkillInitialize() { }

        public override void OnInitialize(TrackAsset aPent)
        {
            base.OnInitialize(aPent);
            SkillInitialize();
        }

        public virtual void InitInput(SkillParameter input)
        {
            InputParam = input;
        }

    }
    
    

}