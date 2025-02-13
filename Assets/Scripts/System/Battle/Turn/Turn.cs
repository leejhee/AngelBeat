public class Turn : State<CharBase>
{
    public enum Side
    {
        None,
        Player,
        Enemy,
        Neutral,
        SideMax
    }

    private CharBase _turnOwner = null;
    private Side _whoseSide = Side.None;

    public CharBase TurnOwner => _turnOwner;
    public Side WhoseSide => _whoseSide;


    public Turn(CharBase TurnOwner)
    {
        _turnOwner = TurnOwner;
        _whoseSide = TurnOwner.GetCharType() == SystemEnum.eCharType.Enemy ?
            Side.Enemy : Side.Player;

    }




}