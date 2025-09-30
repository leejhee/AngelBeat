using Core.Scripts.Foundation.Define;
using GamePlay.Features.Battle.Scripts.Unit;
using GamePlay.Features.Scripts.Battle.Unit;

namespace AngelBeat
{
    /// <summary>
    /// 대미지 전달 단위값
    /// </summary>
    public struct DamageParameter
    {
        public float FinalDamage;
        public CharBase Attacker;
        public CharBase Target;
        public SystemEnum.eSkillType SkillType;
    }
}