using System.Collections.Generic;

public class TurnController
{
    private Queue<Turn> _turnQueue;

    private Turn _currentTurn;

    public Turn CurrentTurn => _currentTurn;
    public CharBase TurnOwner => _currentTurn.TurnOwner;

    public TurnController(List<CharBase> BattleList)
    {
        List<Turn> turns = new List<Turn>();
        foreach (var character in BattleList)
        {
            turns.Add(new Turn(character));
        }
        SortTurn(turns);
    }

    public void SortTurn(List<Turn> turns)
    {
        //턴 계산 코드..
    }

    public void ChangeTurn()
    {
        if(_currentTurn != null)
            _currentTurn.Exit(TurnOwner);

        //if (_turnQueue.Count == 0)
        //    SortTurn();

        _currentTurn = _turnQueue.Dequeue();
        _currentTurn.Enter(TurnOwner);
    }

    public void ChangeTurn(Turn targetTurn)
    {
        if (_currentTurn != null)
            _currentTurn.Exit(TurnOwner);

        _currentTurn = targetTurn;
        _currentTurn.Enter(TurnOwner);

        // 강제 턴 조정 관련한 로직 작성하기.
    }
}