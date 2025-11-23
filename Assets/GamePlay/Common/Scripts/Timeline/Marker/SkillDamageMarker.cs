using Cysharp.Threading.Tasks;
using GamePlay.Common.Scripts.Contracts;
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
            SkillModel model =  SkillBase.SkillModel;

            foreach (IDamageable target in InputParam.Target)
            {
                DamageParameter param = new()
                {
                    Attacker = caster,
                    Target = target,
                    Model = model
                };

                if (target is CharBase character)
                {
                    bool evaded = await character.TryEvade(param);
                    if (evaded) continue; // 회피했으면 알바 아님
                    await character.SkillDamage(param); // 피격 연출 및 idle로 안돌아감
                }
                else
                {
                    // 식 받으면 그거에 따라 damageparameter을 받아서 실행할 것.
                    await target.DamageAsync(1, ct);
                }
                
                
            }
        }

        protected override void SkillInitialize() { }
        
    }
}