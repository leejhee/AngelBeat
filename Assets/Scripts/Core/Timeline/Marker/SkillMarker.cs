using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Client
{
    /// <summary>
    /// 스킬 타임라인용 마커
    /// </summary>
    public class SkillMarker : Marker, INotification
    {
        public PropertyName id => new PropertyName();

        public override void OnInitialize(TrackAsset aPent)
        {
            base.OnInitialize(aPent);
        }
    }
}