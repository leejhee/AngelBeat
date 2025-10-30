using Core.Scripts.Data;
using Core.Scripts.Foundation.Define;
using Core.Scripts.Managers;
using System;
using UnityEngine;

namespace GamePlay.Common.Scripts.Entities.Skills
{
    [Serializable]
    public class SkillModel
    {
        public readonly long SkillIndex; // 데이터상의 인덱스
        public readonly string SkillName;
        public readonly long SkillOwnerID;
        public readonly SystemEnum.eSkillType skillType;
        public readonly Sprite icon;
        public readonly int critCalibration;
        public readonly int skillAccuracy;
        public readonly string prefabName;
        
        public SkillRangeData           skillRange;
        public SkillDamageData          skillDamage;
        public readonly SystemEnum.eSkillUnlock unlock;
        public bool locked = true;
            
        public SkillModel(SkillData skillData)
        {
            SkillIndex = skillData.index;
            SkillName = skillData.skillName;
            SkillOwnerID = skillData.characterID;
            skillType = skillData.skillType;
            
            critCalibration = skillData.skillCritical;
            skillAccuracy = skillData.skillAccuracy;

            //icon = ResourceManager.Instance.LoadAsync<Sprite>(skillData.skillIconImage);
            skillRange = DataManager.Instance.GetData<SkillRangeData>(skillData.skillRangeID);
            skillDamage = DataManager.Instance.GetData<SkillDamageData>(skillData.skillDamage);
            unlock = skillData.unlockCondition;
            prefabName = skillData.skillTimeLine;
        }

        public SkillModel(DokkaebiSkillData skillData)
        {
            SkillIndex = skillData.index;
            SkillName =  skillData.skillName;
            SkillOwnerID = SystemConst.DokkaebiID;
            skillType = skillData.skillType;
            
            critCalibration = skillData.skillCritical;
            skillAccuracy = skillData.skillAccuracy;
            
            skillRange = DataManager.Instance.GetData<SkillRangeData>(skillData.skillRange);
            skillDamage = DataManager.Instance.GetData<SkillDamageData>(skillData.skillDamage);
            unlock = SystemEnum.eSkillUnlock.None;
            locked = false;
            prefabName = skillData.skillTimeLine;
        }
        
    }
    
}