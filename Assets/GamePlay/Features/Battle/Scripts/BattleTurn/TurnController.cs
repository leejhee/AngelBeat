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
        #region Fields
        private int _round = 1;
        private Queue<Turn> _turnQueue = new();
        private readonly List<Turn> _turnBuffer = new();
        private readonly Dictionary<long, int> _actorTurnCounts = new();
        #endregion
        
        #region Properties
        public Turn CurrentTurn { get; private set; }
        public int CurrentRound => _round;
        public CharBase TurnOwner => CurrentTurn?.TurnOwner;
        public Func<UniTask> OnRoundProceeds;
        public event Action OnRoundEnd;
        public IReadOnlyCollection<Turn> TurnCollection => _turnBuffer.AsReadOnly();
        
        #endregion
        
        #region New Events - Testing
        public event Func<RoundEventContext, UniTask> OnRoundProceedAsync;
        public event Func<TurnEventContext, UniTask> OnTurnBeganAsync;
        public event Func<TurnEventContext, UniTask> OnTurnEndedAsync;
        
        #endregion
        
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
            #region End Previous turn & Ending Event
            if (CurrentTurn != null)
            {
                if (TurnOwner && OnTurnEndedAsync != null)
                {
                    long actorID = TurnOwner.GetID();
                    _actorTurnCounts.TryGetValue(actorID, out int actorTurnCount);

                    TurnEventContext endCtx = new(_round, TurnOwner, actorTurnCount);
                    await OnTurnEndedAsync(endCtx);
                }
                
                CurrentTurn?.End();
            }
            #endregion
            
            #region Check Round Ending & Event & Rebuild
            if (_turnQueue.Count == 0)
            {
                OnRoundEnd?.Invoke();
                _round++;
                RebuildTurnQueue();

                if (OnRoundProceedAsync != null)
                {
                    RoundEventContext roundCtx = new(_round);
                    await OnRoundProceedAsync.Invoke(roundCtx);
                }
                //TODO : 하나에 다 묶을 지 선택할 것
                if (OnRoundProceeds != null)
                {
                    await OnRoundProceeds.Invoke();
                }
            }
            #endregion
            
            #region Start Next Turn & Starting Event
            CurrentTurn = _turnQueue.Dequeue();
            while(!CurrentTurn.IsValid)
                CurrentTurn = _turnQueue.Dequeue();

            if (TurnOwner)
            {
                long id = TurnOwner.GetID();
                if (!_actorTurnCounts.TryGetValue(id, out int actorTurnCount))
                    actorTurnCount = 0;
                actorTurnCount++;
                _actorTurnCounts[id] = actorTurnCount;

                if (OnTurnBeganAsync != null)
                {
                    TurnEventContext beginCtx = new(_round, TurnOwner, actorTurnCount);
                    await OnTurnBeganAsync(beginCtx);
                }
            }
            
            CurrentTurn.Begin();
            OnTurnChanged?.Invoke(new TurnModel(CurrentTurn));
            
            #endregion
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
        
        /// <summary>
        /// UID로 해당 캐릭터가 포함된 턴 탐색
        /// </summary>
        public Turn FindTurn(CharBase client)
        {
            return _turnBuffer.Find(x => x.TurnOwner.GetID() == client.GetID());
        }
    }
}
