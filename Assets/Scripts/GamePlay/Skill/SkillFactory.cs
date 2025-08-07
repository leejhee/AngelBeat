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
            var _skillData = global::Core.SingletonObjects.Managers.DataManager.Instance.GetData<SkillData>(skillIndex);

            if (_skillData == null)
            {
                Debug.LogWarning($"CreateSkill : {skillIndex} 스킬 생성 실패.");
                return null;
            }

            skillBase = ResourceManager.Instance.Instantiate<SkillBase>($"Skill/{_skillData.skillTimeLine}");
            skillBase.Init(_skillData);

            return skillBase;
        }
        
        // GetData를 많이 하는 것보다 나을 거 같아서 사용
        public static SkillBase CreateSkill(SkillData skillData)
        {
            if (skillData == null)
            {
                Debug.LogError("왜 매개변수로 null 넣으세요? : CreateSkill(SkillData)");
                return null;
            }
            SkillBase skillBase = ResourceManager.Instance.Instantiate<SkillBase>($"Skill/{skillData.skillTimeLine}");
            skillBase.Init(skillData);
            return skillBase;
        }
    }
}
