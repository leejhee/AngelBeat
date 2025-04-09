using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Playables;

namespace Client
{
    /// <summary>
    /// 스킬용 플레이어블 에셋
    /// </summary>
    public abstract class SkillTimeLinePlayableAsset : PlayableAsset
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
