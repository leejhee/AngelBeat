using UnityEngine;
using UnityEngine.Playables;

namespace AngelBeat
{
    public class SkillMarkerReceiver : MonoBehaviour, INotificationReceiver
    {
        public SkillParameter Input;
        public void OnNotify(Playable origin, INotification notification, object context)
        {
            if (notification is SkillTimeLineMarker skillMarker)
            {
                skillMarker.InitInput(Input);
                skillMarker.MarkerAction();

            }
        }
    }
}