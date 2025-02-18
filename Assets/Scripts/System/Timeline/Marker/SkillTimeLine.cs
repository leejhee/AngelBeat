using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Client
{

    public class SkillTimeLine : MonoBehaviour, INotificationReceiver
    {
        Animator animator;
        private void Awake()
        {

        }

        public void OnNotify(Playable origin, INotification notification, object context)
        {
            SkillTimeLineMarker Skill = notification as SkillTimeLineMarker;
            if (Skill == null)
                return;

            Skill.MarkerAction();


            Debug.Log($"{notification.id}");
            Debug.Log($"{origin}");

            
            if (context == null)
            {
                Debug.Log($"context null");
            }
            else
            {
                Debug.Log($"context : {context.ToString()}");
            }
        }
    }

}