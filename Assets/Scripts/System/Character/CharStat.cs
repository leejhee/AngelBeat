using static SystemEnum;

public class CharStat
{
    private long[] _charStat = new long[(int)eState.MaxCount];

    public CharStat(CharStatData charStat)
    {
        _charStat[(int)eState.STR] = charStat.strength;
        _charStat[(int)eState.NSTR] = charStat.strength;

        _charStat[(int)eState.AGI] = charStat.agility;
        _charStat[(int)eState.NAGI] = charStat.agility;

        _charStat[(int)eState.INT] = charStat.intel;
        _charStat[(int)eState.NINT] = charStat.intel;

        _charStat[(int)eState.Speed] = charStat.speed;
        _charStat[(int)eState.NSpeed] = charStat.speed;

        _charStat[(int)eState.Defence] = charStat.defense;
        _charStat[(int)eState.NDefence] = charStat.defense;
    }

    public long GetStat(eState eState)
    {
        return _charStat[(int)eState];
    }
}