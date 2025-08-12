using Character.Unit;
using Core.Foundation.Define;

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