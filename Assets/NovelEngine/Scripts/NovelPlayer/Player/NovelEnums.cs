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
        Sfx,
        StopSfx,
        Effect,
        ShowCharacter,
        HideCharacter,
        HideAll,
        Clearall,
        Choice,
        Goto,
        Wait,
        If,
        Else,
        HideUI
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
    public enum CompOp
    {
        GreaterThan,       // >
        LessThan,          // <
        GreaterThanOrEqual, // >=
        LessThanOrEqual,    // <=
        Equal,             // ==
        NotEqual,           // !=,
        None
    }
    public enum NovelSound
    {
        Bgm,
        Effect,
        MaxCount
    }
}