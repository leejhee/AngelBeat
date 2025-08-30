namespace GamePlay.Contracts
{
    public readonly struct ActorID
    {
        public readonly int Value;
        public ActorID(int v) => Value = v;
        public override string ToString() => Value.ToString();
    }
}