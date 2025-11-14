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
    
        /// <summary> 이 라운드에서 이 캐릭터의 몇 번째 턴인지</summary>
        public int TurnIndexInRound { get; }

        /// <summary> 이번 턴의 주인 </summary>
        public CharBase Actor { get; }

        /// <summary> 이 Actor가 전투 시작 이후 "몇 번째로 행동하는 턴"인지 </summary>
        public int ActorTurnCount { get; }

        public TurnEventContext(int round, int turnIndexInRound, CharBase actor, int actorTurnCount)
        {
            Round = round;
            TurnIndexInRound = turnIndexInRound;
            Actor = actor;
            ActorTurnCount = actorTurnCount;
        }
    }
    
}