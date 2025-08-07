using Newtonsoft.Json;
using System;

namespace GamePlay.Character.Save
{
    [Serializable]
    public class CharacterProgressSaveData
    {
        #region Skill Locker Part
        [JsonProperty("skillLocker")]
        public SystemEnum.eSkillUnlock skillLocker;
        
        public void SetSkillFlag(SystemEnum.eSkillUnlock changingLocker, bool flag)
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
        public bool GetSkillFlag(SystemEnum.eSkillUnlock target) => skillLocker.HasFlag(target);
        #endregion
    }
}