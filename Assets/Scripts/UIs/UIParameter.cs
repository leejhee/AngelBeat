using Core.SingletonObjects.Managers;

namespace AngelBeat
{
    public abstract class UIParameter : MessageUnit{ }

    public class OnTurnChanged : UIParameter
    {
        public CharBase TurnOwner;
    }
    
    public class OnMoveInput : UIParameter
    {
    }
    
    public class OnTurnEndInput : UIParameter{}
    
}
