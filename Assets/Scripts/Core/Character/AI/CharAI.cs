public class CharAI
{
    public CharBase CharAgent { get; private set; }
    public CharBase CurrentTarget { get; private set; }

    public CharAI(CharBase charAgent)
    {
        CharAgent = charAgent;
    }

    //로직 기록...
}