using GamePlay.Features.Battle.Scripts.Unit;
using System.Collections;
using UnityEngine;

namespace AngelBeat
{
    public class SkillPushTargetMarker : SkillTimeLineMarker
    {
        private Coroutine moveCoroutine;
        private CharBase target;
        [SerializeField] private float movespeed;
        public override void MarkerAction()
        {
            //미는 동작
            target = InputParam.Target[0]; // 무조건 한명 대상
            target.StartCoroutine(PushMove());
        }

        private IEnumerator PushMove()
        { 
            Vector3 dir = (target.transform.position - InputParam.Caster.transform.position).normalized;
            Vector3 dest = dir * 5 + target.transform.position;
            while ((dest - target.transform.position).sqrMagnitude > 0.1f)
            {
                target.transform.position += dir * movespeed * Time.deltaTime;
                yield return null;
            }
        }
    }
}