using Core.Scripts.Data;
using Core.Scripts.Foundation.Define;
using System;
using UnityEngine;

namespace GamePlay.Common.Scripts.Entities.Skills
{
    [Serializable]
    public class SkillModel
    {
        public readonly string      SkillName;
        
        public long                SkillIndex;

        // 가변
        public bool                     IsSkillActive;
        public long                     SkillRange;
        public int                      SkillHitRange;
        public Sprite                   Icon;
        public SystemEnum.ePivot        SkillPivot;
        public SystemEnum.eSkillType    SkillType;
        public int                      Accuracy;
        public int                      CritMultiplier;
        public SkillModel(SkillData skillData)
        {
            SkillIndex = skillData.index;
            SkillRange = skillData.skillRangeID;
            SkillPivot = skillData.skillPivot;
            SkillType = skillData.skillType;
            Accuracy = skillData.skillAccuracy;
            CritMultiplier = skillData.skillCritical;
            
            Icon = Core.Scripts.Managers.ResourceManager.Instance.LoadSprite("Sprites/SkillIcon" + skillData.skillIconImage);
            SkillName = skillData.skillName;
            
            IsSkillActive = false;
            
            
        }
        
        //active 언락의 구독.
        
    }
}