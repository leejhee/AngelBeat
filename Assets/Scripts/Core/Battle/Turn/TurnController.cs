using AngelBeat.Core.SingletonObjects.Managers;
using System.Collections.Generic;

public class TurnController
{
    private readonly Queue<Turn> _turnQueue = new();
    private readonly List<Turn> _turnBuffer = new();

    public Turn CurrentTurn { get; private set; }
    public CharBase TurnOwner => CurrentTurn?.TurnOwner;

    public TurnController()
    {
        InitializeTurnQueue();
    }

    private void InitializeTurnQueue()
    {
        var sorted = BattleCharManager.Instance.GetBattleParticipants();
        foreach (var character in sorted)
        {
            _turnQueue.Enqueue(new Turn(character));
        }
    }

    public void RebuildTurnQueue()
    {
        _turnQueue.Clear();
        InitializeTurnQueue();
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