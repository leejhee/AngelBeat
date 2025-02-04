using UnityEngine;

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

        skillBase = ResourceManager.Instance.Instantiate<SkillBase>($"Skill/{_skillData.skillTimeLineName}");

        if (_skillData == null)
        {
            Debug.LogWarning($"CreateSkill : {skillIndex} 스킬 생성 실패.");
        }
        return skillBase;
    }
}