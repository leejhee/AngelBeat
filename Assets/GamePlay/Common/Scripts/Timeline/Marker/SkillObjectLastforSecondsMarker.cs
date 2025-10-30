using GamePlay.Common.Scripts.Timeline.Marker;
using System.Collections;
using UnityEngine;

namespace AngelBeat
{
    public class SkillObjectLastForSecondsMarker : SkillTimeLineMarker
    {
        [SerializeField] private float lastingSeconds;
        [SerializeField] private GameObject lastingObject;
        [SerializeField] private Vector2 offset;
        
        public override void MarkerAction()
        {
            if (!lastingObject) return;
            InputParam.Caster.StartCoroutine(ObjectLasts());
        }

        public IEnumerator ObjectLasts()
        {
            float timer = 0f;
            GameObject inst = Instantiate(lastingObject, InputParam.Caster.transform);
            inst.transform.position += new Vector3(offset.x, offset.y, 0f);
            while (timer < lastingSeconds)
            {
                yield return null;
            }
            Destroy(inst);
        }
    }
}