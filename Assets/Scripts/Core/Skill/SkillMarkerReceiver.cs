using UnityEngine;
using UnityEngine.Playables;

namespace AngelBeat
{
    public class SkillMarkerReceiver : MonoBehaviour, INotificationReceiver
    {
        public void OnNotify(Playable origin, INotification notification, object context)
        {
            if (notification is SkillTimeLineMarker skillMarker)
            {
                //SkillBase provider = GetComponent<SkillBase>();
                skillMarker.MarkerAction();

            }
        }
    }
}