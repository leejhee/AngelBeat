using GamePlay.Common.Scripts.Skill;
using GamePlay.Features.Battle.Scripts.Unit;
using UnityEngine;
using UnityEngine.Playables;

namespace GamePlay.Common.Scripts.Timeline.PlayableAsset
{
    public abstract class SkillTimeLinePlayableAsset : UnityEngine.Playables.PlayableAsset
    {
        protected CharBase charBase;
        protected SkillBase skillBase;
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            if (charBase == null)
            {
                skillBase = owner.GetComponent<SkillBase>();
                if (skillBase == null)
                    return new();

                charBase = skillBase.CharPlayer;
            }
            return new Playable();
        }
    }
}
