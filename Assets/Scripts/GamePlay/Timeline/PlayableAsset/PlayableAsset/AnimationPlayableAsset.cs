using Character.Unit;
using GamePlay.Skill;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Playables;
using UnityEngine.Animations;

namespace AngelBeat
{
    public class AnimationPlayableAsset: SkillTimeLinePlayableAsset
    {
        [SerializeField]
        private AnimationClip animationClip;

        public AnimationClip AnimationClip => animationClip;
        public override double duration => 
            animationClip != null ? animationClip.length : base.duration;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            base.CreatePlayable(graph, owner);

            var playableBehaviour = new AnimationPlayableBehaviour();
            
            SkillBase skill = owner.GetComponent<SkillBase>();
            CharBase player = skill.CharPlayer;
            playableBehaviour.InitBehaviour(player, skill);
            
            playableBehaviour.animator = player.CharAnim.Animator;
            playableBehaviour.animationClip = animationClip;
            var scriptPlayable = ScriptPlayable<AnimationPlayableBehaviour>.Create(graph, playableBehaviour);

            // AnimationClipPlayable ����
            var animationPlayable = AnimationClipPlayable.Create(graph, animationClip);

            // �÷��̺��� ����
            scriptPlayable.AddInput(animationPlayable, 0, 1);

            return scriptPlayable;
        }
    }
}