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
        public SystemEnum.eSkillUnlock eSkillLocker;
        
        public void SetSkillFlag(SystemEnum.eSkillUnlock changingLocker, bool flag)
        {
            if (flag)
            {
                eSkillLocker |= changingLocker;
            }
            else
            {
                eSkillLocker &= ~changingLocker;
            }
        }
        public bool GetSkillFlag(SystemEnum.eSkillUnlock target) => eSkillLocker.HasFlag(target);
        #endregion
    }
}