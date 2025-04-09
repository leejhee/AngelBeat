using static SystemEnum;

public class CharStat
{
    private long[] _charStat = new long[(int)eStats.eMax];

    public CharStat(CharStatData charStat)
    {
        _charStat[(int)eStats.STR] = charStat.strength;
        _charStat[(int)eStats.NSTR] = charStat.strength;

        _charStat[(int)eStats.AGI] = charStat.agility;
        _charStat[(int)eStats.NAGI] = charStat.agility;

        _charStat[(int)eStats.INT] = charStat.intel;
        _charStat[(int)eStats.NINT] = charStat.intel;

        _charStat[(int)eStats.Speed] = charStat.speed;
        _charStat[(int)eStats.NSpeed] = charStat.speed;

        _charStat[(int)eStats.Defence] = charStat.defense;
        _charStat[(int)eStats.NDefence] = charStat.defense;

    }

    public long GetStat(eStats eState)
    {
        return _charStat[(int)eState];
    }
}