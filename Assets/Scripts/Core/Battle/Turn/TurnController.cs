using AngelBeat.Core.SingletonObjects.Managers;
using System.Collections.Generic;

namespace AngelBeat
{
    public class TurnController
    {
        private readonly Queue<Turn> _turnQueue = new();
        private readonly List<Turn> _turnBuffer = new();

        public Turn CurrentTurn { get; private set; }
        public CharBase TurnOwner => CurrentTurn?.TurnOwner;

        public TurnController(List<CharBase> battleMembers)
        {
            InitializeTurnQueue(battleMembers);
        }

        private void InitializeTurnQueue(List<CharBase> battleMembers)
        {
            //var sorted = BattleCharManager.Instance.GetBattleParticipants();
            foreach (var character in battleMembers)
            {
                _turnQueue.Enqueue(new Turn(character));
            }
        }

        private void InitializeTurnQueue(List<Turn> buffer)
        {
            
        }
        
        public void RebuildTurnQueue()
        {
            _turnQueue.Clear();
            InitializeTurnQueue(_turnBuffer);
        }

        public void ChangeTurn()
        {
            CurrentTurn?.End();

            if (_turnQueue.Count == 0)
                RebuildTurnQueue();

            CurrentTurn = _turnQueue.Dequeue();
            CurrentTurn.Begin();
        
        }

        public void ChangeTurn(Turn targetTurn)
        {
            CurrentTurn?.End();

            CurrentTurn = targetTurn;
            CurrentTurn.Begin();

            // 강제 턴 조정 관련한 로직 작성하기.
        }
    }
}
