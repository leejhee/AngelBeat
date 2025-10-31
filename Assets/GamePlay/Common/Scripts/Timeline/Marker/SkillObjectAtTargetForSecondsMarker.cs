﻿using System.Threading;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay.Common.Scripts.Timeline.Marker
{
    public class SkillObjectAtTargetForSecondsMarker : SkillTimeLineMarker
    {
        [SerializeField] private GameObject lastingObject;
        [SerializeField] private float seconds;
        [SerializeField] private Vector2 offset;
        
        public override async UniTask BuildTaskAsync(CancellationToken ct)
        {
            if (!lastingObject || InputParam.Target.Count == 0) 
                return;

            Queue<GameObject> instances = new();
            
            foreach (var target in InputParam.Target)
            {
                GameObject inst = Instantiate(lastingObject, target.transform);
                inst.transform.position += new Vector3(offset.x, offset.y, 0f);
                instances.Enqueue(inst);
            }
            
            float timer = 0f;
            while (timer < seconds)
            {
                timer += Time.deltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }
            
            while (instances.TryDequeue(out GameObject go))
            {
                Destroy(go);
            }
        }
    }
}