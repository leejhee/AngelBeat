using System;
using System.Collections.Generic;
using Utils.Core.Random;
using static SystemEnum;

namespace Core.GameSave
{
    [Serializable]
    public class GameSlotData
    {
        #region System Part
        public DateTime lastSavedTime = DateTime.Now;
        public GameState lastGameState = GameState.None;
        public Dictionary<GameState, ISavableEntity> savedDict = new();
        
        #endregion        
        
        
        #region Skill Locker Part
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
        
        #region General Fiels
        
        public GameRandom gameRnd;
        
        #endregion
        public GameSlotData()
        {
            gameRnd = new GameRandom((ulong)lastSavedTime.Ticks);
        }
        
        
        
    }
}