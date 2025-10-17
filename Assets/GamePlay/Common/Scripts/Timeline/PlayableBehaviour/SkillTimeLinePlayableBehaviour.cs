using GamePlay.Common.Scripts.Skill;
using GamePlay.Features.Battle.Scripts.Unit;

namespace GamePlay.Common.Scripts.Timeline.PlayableBehaviour
{
    public abstract class SkillTimeLinePlayableBehaviour : UnityEngine.Playables.PlayableBehaviour
    {
        public CharBase charBase;
        public SkillBase skillBase;
        
        public virtual void InitBehaviour(CharBase character, SkillBase skill)
        {
            this.charBase = character;
            this.skillBase = skill;
        }
    }
}