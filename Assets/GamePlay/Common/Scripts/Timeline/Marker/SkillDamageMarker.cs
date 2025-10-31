﻿using Cysharp.Threading.Tasks;
using GamePlay.Common.Scripts.Entities.Character.Components;
using GamePlay.Common.Scripts.Entities.Skills;
using GamePlay.Features.Battle.Scripts.Unit;
using System.Threading;

namespace GamePlay.Common.Scripts.Timeline.Marker
{
    public class SkillDamageMarker : SkillTimeLineMarker
    {
        public override async UniTask BuildTaskAsync(CancellationToken ct)
        {
            CharBase caster =   InputParam.Caster;
            SkillModel model =  InputParam.Model;

            foreach (var target in InputParam.Target)
            {
                DamageParameter param = new()
                {
                    Attacker = caster,
                    Target = target,
                    Model = model
                };
                bool evaded = await target.TryEvade(param);
                if (evaded) continue; // 회피했으면 알바 아님
                
                await target.SkillDamage(param); // 피격 연출 및 idle로 안돌아감
            }
        }

        protected override void SkillInitialize() { }
        
    }
}