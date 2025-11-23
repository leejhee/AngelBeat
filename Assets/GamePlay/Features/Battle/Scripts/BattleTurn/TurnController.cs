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
        private int _round;
        private Queue<Turn> _turnQueue = new();
        private readonly List<Turn> _turnBuffer = new();
        private readonly Dictionary<long, int> _actorTurnCounts = new();
        #endregion
        
        #region Properties
        public int CurrentRound => _round;
        public Turn CurrentTurn { get; private set; }
        public CharBase TurnOwner => CurrentTurn?.TurnOwner;
        //public event Func<UniTask> OnRoundProceeds;
        public IReadOnlyCollection<Turn> TurnCollection => _turnBuffer.AsReadOnly();
        
        #endregion
        
        #region New Events - Testing
        
        // Domain Event
        public event Func<RoundEventContext, UniTask> OnRoundProceedAsync;
        public event Func<TurnEventContext, UniTask> OnTurnBeganAsync;
        public event Func<TurnEventContext, UniTask> OnTurnEndedAsync;
        public event Action OnRoundEnd;
        // UI Event 
        public event Action<TurnChangedDTO> OnTurnChanged;
        public event Action<TurnOrderDTO> OnTurnOrderChanged;
        public event Action<TurnActionDTO> OnCurrentTurnActionChanged;
        #endregion
        
 
        private void InitializeTurnQueue(List<CharBase> battleMembers)
        {
            foreach (CharBase character in battleMembers)
            {
                Turn newTurn = new(character);
                newTurn.OnAITurnCompleted += () => ChangeTurn().Forget();
                newTurn.OnTurnAction += dto =>
                {
                    if (newTurn == CurrentTurn)
                        OnCurrentTurnActionChanged?.Invoke(dto);
                };
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
                    if (OnTurnEndedAsync != null)
                    {
                        foreach (var d in OnTurnEndedAsync.GetInvocationList())
                        {
                            if (d is Func<TurnEventContext, UniTask> handler)
                            {
                                await handler(endCtx);
                            }
                        }
                    }
                    
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
                
                OnTurnOrderChanged?.Invoke(new TurnOrderDTO(TurnCollection));
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
                    foreach (var d in OnTurnBeganAsync.GetInvocationList())
                    {
                        if (d is Func<TurnEventContext, UniTask> handler)
                        {
                            await handler(beginCtx);
                        }
                    }
                }
            }
            
            CurrentTurn.Begin();
            OnTurnChanged?.Invoke(new TurnChangedDTO(_round, TurnOwner));
            #endregion
        }
        
        [Obsolete("동기 턴 변경은 현재 사용처 소실. 비동기로 사용할 것.")]
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
