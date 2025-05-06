using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace AngelBeat
{
    /// <summary>
    /// ��ų Ÿ�Ӷ��ο� ��Ŀ
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