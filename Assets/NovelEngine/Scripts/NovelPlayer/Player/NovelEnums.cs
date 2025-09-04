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
        SFX,
        Effect,
        ShowCharacter,
        HideCharacter,
        Clearall,
        Choice,
        Goto
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
    public enum BGMCommandType
    {
        Play,
        Stop
    }
}