using AngelBeat.Core.SingletonObjects.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using static AngelBeat.TurnComparisonMethods;

namespace AngelBeat
{
    /// <summary>
    /// 전투 내 턴 관리 클래스
    /// </summary>
    public class TurnController
    {
        private int _round = 1;
        private Queue<Turn> _turnQueue = new();
        private List<Turn> _turnBuffer = new();

        public Turn CurrentTurn { get; private set; }
        public CharBase TurnOwner => CurrentTurn?.TurnOwner;
        public event Action OnRoundProceeds;
        
        public TurnController(List<CharBase> battleMembers)
        {
            InitializeTurnQueue(battleMembers);
        }

        private void InitializeTurnQueue(List<CharBase> battleMembers)
        {
            foreach (var character in battleMembers)
            {
                _turnBuffer.Add(new Turn(character));
            }
            _turnBuffer.Sort(new TurnComparer(VanillaComparer));
            foreach (var turn in _turnBuffer)
            {
                _turnQueue.Enqueue(turn);
            }
        }

        private void InitializeTurnQueue(List<Turn> buffer)
        {
            buffer.Sort(new TurnComparer(VanillaComparer));
            foreach (var turn in buffer)
            {
                _turnQueue.Enqueue(turn);
            }
        }

        private void RebuildTurnQueue()
        {
            _turnQueue.Clear();
            List<CharBase> newRoundMembers = BattleCharManager.Instance.GetBattleMembers();
            InitializeTurnQueue(newRoundMembers);
        }
        
        public void RefreshTurn() => RebuildTurnQueue();
        
        public void ChangeTurn()
        {
            CurrentTurn?.End();

            if (_turnQueue.Count == 0)
            {
                _round++;
                OnRoundProceeds?.Invoke();
                RebuildTurnQueue();
            }
            
            CurrentTurn = _turnQueue.Dequeue();
            while(!CurrentTurn.IsValid)
                CurrentTurn = _turnQueue.Dequeue();
            
            CurrentTurn.Begin();
        }
        
        public void ChangeTurn(Turn targetTurn)
        {
            CurrentTurn?.End();
            if (_turnQueue.Contains(targetTurn))
                _turnQueue = new Queue<Turn>(_turnQueue.Where(t => t != targetTurn));
            CurrentTurn = targetTurn;
            CurrentTurn.Begin();

            // 강제 턴 조정 관련한 로직 작성하기.
        }
    }
}
