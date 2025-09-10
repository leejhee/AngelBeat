using Core.Scripts.Data;
using Core.Scripts.Foundation.Define;
using System;
using UnityEngine;

namespace GamePlay.Skill
{
    [Serializable]
    public class SkillModel
    {
        private SkillData _skillData;
        public readonly string      SkillName;
        
        public long                SkillIndex => _skillData.index;

        // 가변
        public bool                     IsSkillActive;
        public int                      SkillRange;
        public int                      SkillHitRange;
        public Sprite                   Icon;
        public SystemEnum.ePivot        SkillPivot;
        public SystemEnum.eSkillType    SkillType;
        public float                    DamageCalibration;
        public int                      Accuracy;
        public int                      CritMultiplier;
        public SkillModel(SkillData skillData)
        {
            _skillData = skillData;
            
            SkillRange = skillData.skillRange;
            SkillPivot = skillData.skillPivot;
            SkillHitRange = skillData.skillPivotRange;
            SkillType = skillData.skillType;
            DamageCalibration = skillData.damageCalibration;
            Accuracy = skillData.skillAccuracy;
            CritMultiplier = skillData.skillCritical;
            
            Icon = Core.Scripts.Managers.ResourceManager.Instance.LoadSprite("Sprites/SkillIcon" + skillData.skillIconImage);
            SkillName = skillData.skillName;
            
            IsSkillActive = false;
            
            
        }
        
        //active 언락의 구독.
        
    }
}