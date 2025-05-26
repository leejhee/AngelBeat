using System;

namespace Core.GameSave
{
    [Serializable]
    public class GameSlotData
    {
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
    }
}