using GamePlay.Common.Scripts.Entities.Skills;
using GamePlay.Features.Battle.Scripts.Unit;

namespace GamePlay.Common.Scripts.Entities.Character.Components
{
    /// <summary>
    /// 대미지 전달 단위값
    /// </summary>
    public struct DamageParameter
    {
        public CharBase Attacker;
        public CharBase Target;
        public SkillModel Model;
    }
}