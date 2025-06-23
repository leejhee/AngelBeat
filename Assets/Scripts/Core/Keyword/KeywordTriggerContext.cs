namespace AngelBeat
{
    // TODO : 추후 키워드 발동에 필요한 attribute를 추가하면 됨.
    /// <summary>
    /// Owner : 키워드를 가지고 있는 캐릭터
    /// Target : 키워드 작동의 대상이 되는 캐릭터
    /// </summary>
    public class KeywordTriggerContext
    {
        public CharBase Owner;
        public CharBase Target;
    }
}