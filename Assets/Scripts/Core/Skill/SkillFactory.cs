using UnityEngine;

namespace AngelBeat
{
    public static class SkillFactory
    {
        public static SkillBase CreateSkill(string skillName)
        {
            SkillBase skillBase = null;
            skillBase = ResourceManager.Instance.Instantiate<SkillBase>($"Skill/{skillName}");
            return skillBase;
        }
        public static SkillBase CreateSkill(long skillIndex)
        {
            SkillBase skillBase = null;
            var _skillData = DataManager.Instance.GetData<SkillData>(skillIndex);

            if (_skillData == null)
            {
                Debug.LogWarning($"CreateSkill : {skillIndex} 스킬 생성 실패.");
                return null;
            }

            skillBase = ResourceManager.Instance.Instantiate<SkillBase>($"Skill/{_skillData.skillTimeLine}");
            skillBase.Init(_skillData);

            if (_skillData == null)
            {
                Debug.LogWarning($"CreateSkill : {skillIndex} 스킬 생성 실패.");
            }
            return skillBase;
        }
    }
}
