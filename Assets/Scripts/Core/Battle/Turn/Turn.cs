using System;

public class Turn
{
    public enum Side { None, Player, Enemy, Neutral, SideMax }

    public CharBase TurnOwner { get; private set; }
    public Side WhoseSide { get; private set; }

    public Action OnBeginTurn =     delegate { };
    public Action OnProcessTurn =   delegate { };
    public Action OnEndTurn =       delegate { };

    public Turn(CharBase TurnOwner)
    {
        this.TurnOwner = TurnOwner;
        WhoseSide = TurnOwner.GetCharType() == SystemEnum.eCharType.Enemy ?
            Side.Enemy : Side.Player;

        // Execution 구조 짜면 저기다 다 구독해줘야한다.
        // 반격 시스템이 있다고 했기 때문에, message system을 사용해야 할 듯 하다.
    }

    public void Begin() => OnBeginTurn();
    public void Process() => OnProcessTurn();
    public void End() => OnEndTurn();

}