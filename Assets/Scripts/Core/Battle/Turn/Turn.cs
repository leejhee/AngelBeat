using AngelBeat.Core;
using AngelBeat.Core.SingletonObjects.Managers;
using System;
using UnityEngine;

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
            
            // Ready Turn for Owner.
            _onBeginTurn += () =>
            {
                // Control Camera.
                Vector3 cameraPos = Camera.main.transform.position;
                Vector3 charPos = TurnOwner.transform.position;
                Camera.main.transform.position = new Vector3(charPos.x, charPos.y, cameraPos.z);
                
                // Control UI.
                EventBus.Instance.SendMessage(new OnTurnChanged { TurnOwner = TurnOwner });
                
                // Control Logic.
                
            };
            
        }

        public void Begin() => _onBeginTurn();
        public void Process() => _onProcessTurn();
        public void End() => _onEndTurn();
        

    }
}
