using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Client
{
    [TrackColor(0.1f, 0.6f, 0.1f)]
    [TrackBindingType(typeof(GameObject))]
    public class CustomTrack : TrackAsset
    {
     
        public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
            base.GatherProperties(director, driver);
        
        }
    }
}