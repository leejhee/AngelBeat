using Core.Scripts.Data;
using Cysharp.Threading.Tasks;
using GamePlay.Common.Scripts.Contracts;
using GamePlay.Common.Scripts.Entities.Character.Components;
using GamePlay.Common.Scripts.Entities.Skills;
using GamePlay.Features.Battle.Scripts.BattleAction;
using GamePlay.Features.Battle.Scripts.Unit;
using System.Threading;
using UnityEngine;

namespace GamePlay.Common.Scripts.Timeline.Marker
{
    public class SkillPushWithDamageMarker : SkillTimeLineMarker
    {
        public override async UniTask BuildTaskAsync(CancellationToken ct)
        {
            CharBase caster = InputParam.Caster;
            IDamageable target = InputParam.Target[0]; // 단일 타겟이라고 가정.
            var grid = InputParam.Grid;
            SkillModel model = SkillBase.SkillModel;
            
            DamageParameter param = new()
            {
                Attacker = caster,
                Target = target,
                Model = model
            };

            if (target is CharBase character)
            {
                bool evaded = await character.TryEvade(param);
                if (evaded) return; // 회피했으면 알바 아님
            
                //=================맞았을 때================//

                await character.SkillDamage(param, false, true); // 피격 연출 및 idle로 안돌아감
            
                Vector2Int pivot = grid.WorldToCell(caster.CharTransform.position);
                Vector2Int targetCell = grid.WorldToCell(character.CharTransform.position);
                var pushResult = PushEngine.ComputePushResult(pivot, targetCell, grid);
 
                // 밀치기 실행 (연출 포함, 추가 피격 애니메이션 없이)
                await PushEngine.ApplyPushResult(character, pushResult, grid, ct, true, true);
                character.CharReturnIdle(); // 강제 Idle로 귀환
            }
            else
            {
                await target.DamageAsync(1, ct);
            }
            
        }
    }
}