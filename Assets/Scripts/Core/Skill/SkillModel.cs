using System;
using UnityEngine;

namespace AngelBeat
{
    [Serializable]
    public class SkillModel
    {
        private SkillData _skillData;
        public readonly string      SkillName;
        
        // 가변
        public bool                 IsSkillActive;
        public int                  SkillRange;
        public int                  SkillHitRange;
        public Sprite               Icon;
        public SystemEnum.ePivot    SkillPivot;

        public SkillModel(SkillData skillData)
        {
            _skillData = skillData;
            
            SkillRange = skillData.skillRange;
            SkillPivot = skillData.skillPivot;
            SkillHitRange = skillData.skillPivotRange;
            Icon = ResourceManager.Instance.LoadSprite("Sprites/SkillIcon" + skillData.skillIconImage);
            SkillName = skillData.skillName;
            
            IsSkillActive = false;
            
            
        }
        
        //active 언락의 구독.
        
    }
}