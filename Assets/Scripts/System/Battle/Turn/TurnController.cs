using System.Collections.Generic;

public class TurnController
{
    private List<CharBase> _participants;
    private Turn _currentTurn;

    public Turn CurrentTurn => _currentTurn;
    public CharBase TurnOwner => _currentTurn.TurnOwner;

    public TurnController()
    {

    }

    // 턴 계산 여기서 비교자 사용해서 할거임
    public void InitController()
    {
        //턴 계산 코드..
        
    }

    public void ChangeTurn(Turn targetTurn)
    {
        _currentTurn.Exit(TurnOwner);
        _currentTurn = targetTurn;
        _currentTurn.Enter(TurnOwner);
    }
}