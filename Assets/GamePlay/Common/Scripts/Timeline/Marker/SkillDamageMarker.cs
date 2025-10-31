using AngelBeat;
using Core.Scripts.Data;
using Cysharp.Threading.Tasks;
using GamePlay.Common.Scripts.Entities.Character.Components;
using GamePlay.Features.Battle.Scripts.Unit;
using System.Threading;
using UnityEngine;

namespace GamePlay.Common.Scripts.Timeline.Marker
{
    public class SkillDamageMarker : SkillTimeLineMarker
    {
        public override async UniTask BuildTaskAsync(CancellationToken ct)
        {
            CharBase caster = InputParam.Caster;
            foreach (CharBase target in InputParam.Target)
            {
                Debug.Log(target.CharInfo.Name);
                
                if (!target) continue;

                SkillDamageData damage = InputParam.Model.skillDamage;
                if (damage.index == 0)
                {
                    Debug.LogWarning($"해당 스킬 {InputParam.Model.SkillIndex}번은 대미지를 입히지 않습니다.");
                    return;
                }
                
                DamageParameter param = new()
                {
                    Attacker = caster,
                    Target = target,
                    Model = InputParam.Model
                };

                await target.SkillDamage(param);
            }
        }

        protected override void SkillInitialize() { }
        
    }
}