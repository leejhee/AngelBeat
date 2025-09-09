namespace novel
{
    public enum DialogoueType
    {
        CommandLine,
        NormalLine,
        PersonLine,
        LabelLine,
        CommentLine,
        None
    }
    public enum CommandType
    {
        None,
        Background,
        BGM,
        StopBGM,
        SFX,
        StopSFX,
        Effect,
        ShowCharacter,
        HideCharacter,
        HideAll,
        Clearall,
        Choice,
        Goto,
        Wait,
        If,
        Else
    }
    public enum CharacterName
    {
        DonQuixote,
        Gosegu,
        Jururu
    }
    public enum CharCommandType
    {
        Show,
        Hide,
        HideAll,
        Fade,
        Effect
    }
    public enum CompOP
    {
        GreaterThan,       // >
        LessThan,          // <
        GreaterThanOrEqual, // >=
        LessThanOrEqual,    // <=
        Equal,             // ==
        NotEqual,           // !=,
        None
    }
    public enum SoundType
    {
        BGM,
        SFX
    }
}