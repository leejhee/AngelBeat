using GamePlay.Features.Battle.Scripts.Unit;

namespace GamePlay.Features.Battle.Scripts.BattleTurn
{
    public readonly struct RoundEventContext
    {
        public int Round { get; }

        public RoundEventContext(int round)
        {
            Round = round;
        }
    }
    
    public sealed class TurnEventContext
    {
        /// <summary> 현재 라운드 번호 </summary>
        public int Round { get; }

        /// <summary> 이번 턴의 주인 </summary>
        public CharBase Actor { get; }

        /// <summary> 이 Actor가 전투 시작 이후 "몇 번째로 행동하는 턴"인지 </summary>
        public int ActorTurnCount { get; }

        public TurnEventContext(int round, CharBase actor, int actorTurnCount)
        {
            Round = round;
            Actor = actor;
            ActorTurnCount = actorTurnCount;
        }
    }
    
}