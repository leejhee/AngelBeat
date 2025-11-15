using UnityEngine;

namespace GamePlay.Features.Battle.Scripts.Tutorial
{
    public enum GuideAnchor
    {
        ScreenTop,
        Actor,
    }
    
    [System.Serializable]
    public class BattleTutorialGuidePage
    {
        [TextArea]
        public string text;

        public GuideAnchor anchor;
        public bool focusActor;
    }
}