public class CharMonster : CharBase
{
    private CharAI _charAI;

    protected override SystemEnum.eCharType CharType => SystemEnum.eCharType.Enemy;
    protected override void CharInit()
    {
        base.CharInit();
        BattleCharManager.Instance.SetChar<CharMonster>(this);
        _charAI = new(this);

    }

    
}