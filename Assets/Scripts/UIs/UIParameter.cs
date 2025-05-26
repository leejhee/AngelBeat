using AngelBeat.Core.SingletonObjects.Managers;
using static SystemEnum;

namespace AngelBeat
{
    public abstract class UIParameter : MessageUnit{ }
    
    #region On Model Change
    public class OnTurnChanged : UIParameter
    {
        public CharBase TurnOwner;
    }
    
    public class OnKeywordChange : UIParameter
    {
        public eKeyword Keyword;
        public CharBase TargetChar; // 필요한가?
    }
    
    #endregion
    
    #region On UI Input
    public class OnMoveInput : UIParameter{}
    public class OnTurnEndInput : UIParameter{}
    public class OnSkillInput : UIParameter
    {
        public long PlaySkillIndex;
    }
    #endregion
}
