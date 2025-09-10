namespace GamePlay.Contracts
{
    public enum TriggerType
    {
        SoT,
        EoT,
        OnHit,
        OnAttack,
        OnEnterTile
    }

    public readonly struct KeywordTriggerContext
    {
        public readonly ActorID Actor;
        public readonly ActorID? Target;
        public readonly TriggerType Trigger;
        public readonly object Payload;
        public KeywordTriggerContext(ActorID actor, TriggerType type, ActorID? target = null, object payload = null) {
            Actor = actor; Trigger = type; Target = target; Payload = payload;
        }
    }
    
    public interface IKeywordEffectPort {
        void DealDamage(ActorID src, ActorID dst, int amount);
        void ModifyStat(ActorID who, int statId, float delta, int? duration = null);
        void AddKeyword(ActorID who, int keywordId, int stacks, int duration);
    }
    
    public struct KeywordRuntime {
        public int Stacks;
        public int Duration;
        public int Value; 
        public KeywordRuntime(int stacks, int duration, int value = 0) { Stacks = stacks; Duration = duration; Value = value; }
    }
}