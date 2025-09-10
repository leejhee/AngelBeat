using System.Collections.Generic;

namespace AngelBeat
{
    public interface IMapLoader
    {
        public StageField GetBattleField(string stageName=null);
    }
    
}