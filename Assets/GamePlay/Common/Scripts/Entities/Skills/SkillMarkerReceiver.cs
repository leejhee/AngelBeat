using AngelBeat;
using GamePlay.Common.Scripts.Skill;
using GamePlay.Features.Scripts.Skill;
using UnityEngine;
using UnityEngine.Playables;

namespace GamePlay.Skill
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