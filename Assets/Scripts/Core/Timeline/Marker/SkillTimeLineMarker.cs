using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Timeline;
using UnityEngine.Playables;

namespace Client
{
    /// <summary>
    /// 스킬 타임라인용 마커
    /// </summary>
    public class SkillTimeLineMarker : Marker, INotification
    {
        public PropertyName id => new PropertyName("SkillTimeLineMarker");
        protected SkillParameter inputParam;

        public virtual void  MarkerAction()
        {

        }

        public virtual void SkillInitialize()
        {

        }

        public override void OnInitialize(TrackAsset aPent)
        {
            base.OnInitialize(aPent);
            SkillInitialize();
        }

        public virtual void InitInput(SkillParameter input)
        {
            inputParam = input;
        }

    }
    
    

}