using Core.Scripts.Foundation.Define;
using Newtonsoft.Json;
using System;

namespace GamePlay.Character.Save
{
    [Serializable]
    public class CharacterProgressSaveData
    {
        #region Skill Locker Part
        [JsonProperty("skillLocker")]
        public SystemEnum.SkillUnlock skillLocker;
        
        public void SetSkillFlag(SystemEnum.SkillUnlock changingLocker, bool flag)
        {
            if (flag)
            {
                skillLocker |= changingLocker;
            }
            else
            {
                skillLocker &= ~changingLocker;
            }
        }
        public bool GetSkillFlag(SystemEnum.SkillUnlock target) => skillLocker.HasFlag(target);
        #endregion
    }
}