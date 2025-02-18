public class CharMonster : CharBase
{
    private CharAI _charAI;

    protected override SystemEnum.eCharType CharType => SystemEnum.eCharType.Enemy;
    protected override void CharInit()
    {
        base.CharInit();
        CharManager.Instance.SetChar<CharMonster>(this);
        _charAI = new(this);

    }

    
}