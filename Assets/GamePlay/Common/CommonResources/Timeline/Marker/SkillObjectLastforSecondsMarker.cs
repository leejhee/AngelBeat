using System.Collections;
using UnityEngine;

namespace AngelBeat
{
    public class SkillObjectLastForSecondsMarker : SkillTimeLineMarker
    {
        [SerializeField] private float lastingSeconds;
        [SerializeField] private GameObject lastingObject;
        public override void MarkerAction()
        {
            if (!lastingObject) return;
            InputParam.Caster.StartCoroutine(ObjectLasts());
        }

        public IEnumerator ObjectLasts()
        {
            float timer = 0f;
            GameObject inst = Instantiate(lastingObject, InputParam.Caster.transform);
            while (timer < lastingSeconds)
            {
                yield return null;
            }
            Destroy(inst);
        }
    }
}