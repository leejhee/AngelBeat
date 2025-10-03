using GamePlay.Features.Battle.Scripts.Unit;
using GamePlay.Features.Scripts.Skill;
using UnityEngine;
using UnityEngine.Playables;

namespace GamePlay.Timeline.PlayableAsset.PlayableAsset
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
