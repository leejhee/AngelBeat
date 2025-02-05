using static SystemEnum;

public class CharStat
{
    private long[] _charStat = new long[(int)eState.MaxCount];

    public CharStat(CharStatData charStat)
    {
        _charStat[(int)eState.STR] = charStat.strength;
        _charStat[(int)eState.AGI] = charStat.agility;
        _charStat[(int)eState.INT] = charStat.intel;
    }

    public long GetStat(eState eState)
    {
        return _charStat[(int)eState];
    }
}