using AngelBeat;
using Cysharp.Threading.Tasks;
using GamePlay.Features.Battle.Scripts.Unit;
using System;
using System.Collections.Generic;
using System.Linq;
using static GamePlay.Features.Battle.Scripts.BattleTurn.TurnComparisonMethods;

namespace GamePlay.Features.Battle.Scripts.BattleTurn
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
        public Func<UniTask> OnRoundProceeds;
        public event Action OnRoundEnd;
        public IReadOnlyCollection<Turn> TurnCollection => _turnBuffer.AsReadOnly();

        #region UI Model

        public class TurnModel
        {
            public readonly Turn Turn;
            public TurnModel(Turn turn) =>  Turn = turn;
        }

        #endregion        
 
        private void InitializeTurnQueue(List<CharBase> battleMembers)
        {
            foreach (var character in battleMembers)
            {
                Turn newTurn = new BattleTurn.Turn(character);
                
                newTurn.OnAITurnCompleted += () => ChangeTurn().Forget();
                
                _turnBuffer.Add(newTurn);
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
            _turnBuffer.Clear();
            List<CharBase> newRoundMembers = BattleCharManager.Instance.GetBattleMembers();
            InitializeTurnQueue(newRoundMembers);
        }
        
        public void RefreshTurn() => RebuildTurnQueue();
        
        public Action<TurnModel> OnTurnChanged;
        public async UniTask ChangeTurn()
        {
            CurrentTurn?.End();
            
            
            if (_turnQueue.Count == 0)
            {
                OnRoundEnd?.Invoke();
                _round++;
                RebuildTurnQueue();
                if (OnRoundProceeds != null)
                {
                    await OnRoundProceeds.Invoke();
                }
            }
            
            CurrentTurn = _turnQueue.Dequeue();
            while(!CurrentTurn.IsValid)
                CurrentTurn = _turnQueue.Dequeue();
        
            CurrentTurn.Begin();
            OnTurnChanged?.Invoke(new TurnModel(CurrentTurn));
        }
        
        public void ChangeTurn(BattleTurn.Turn targetTurn)
        {
            CurrentTurn?.End();
            if (_turnQueue.Contains(targetTurn))
                _turnQueue = new Queue<BattleTurn.Turn>(_turnQueue.Where(t => t != targetTurn));
            CurrentTurn = targetTurn;
            CurrentTurn.Begin();

            // 강제 턴 조정 관련한 로직 작성하기.
        }
    }
}
