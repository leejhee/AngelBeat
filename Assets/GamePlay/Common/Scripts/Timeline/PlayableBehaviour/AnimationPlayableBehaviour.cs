using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace GamePlay.Common.Scripts.Timeline.PlayableBehaviour
{
    public class AnimationPlayableBehaviour: SkillTimeLinePlayableBehaviour
    {
        public Animator animator { get; set; }
        public AnimationClip animationClip { get; set; }

        PlayableGraph playableGraph;

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            playableGraph = PlayableGraph.Create("AnimationPlayable");

            AnimationClipPlayable clipPlayable = AnimationClipPlayable.Create(playableGraph, animationClip);

            var output = AnimationPlayableOutput.Create(playableGraph, "Animation", animator);
            output.SetSourcePlayable(clipPlayable);

            playableGraph.Play();
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            if (playableGraph.IsValid())
            {
                playableGraph.Stop();
                playableGraph.Destroy(); 
            }
            //animator.Play("IDLE",0,0);
        }
    }
}