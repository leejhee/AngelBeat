using AngelBeat.Core;
using AngelBeat.Core.SingletonObjects.Managers;
using System;

namespace AngelBeat
{
    public class Turn
    {
        public enum Side { None, Player, Enemy, Neutral, SideMax }

        public CharBase TurnOwner { get; private set; }
        public Side WhoseSide { get; private set; }

        private readonly Action _onBeginTurn =     delegate { };
        private readonly Action _onProcessTurn =   delegate { };
        private readonly Action _onEndTurn =       delegate { };

        public Turn(CharBase turnOwner)
        {
            this.TurnOwner = turnOwner;
            WhoseSide = turnOwner.GetCharType() == SystemEnum.eCharType.Enemy ?
                Side.Enemy : Side.Player;
            
            // Ready for UI Change
            _onBeginTurn += () => EventBus.Instance.SendMessage(new OnTurnChanged { TurnOwner = TurnOwner });
        }

        public void Begin() => _onBeginTurn();
        public void Process() => _onProcessTurn();
        public void End() => _onEndTurn();
        

    }
}
