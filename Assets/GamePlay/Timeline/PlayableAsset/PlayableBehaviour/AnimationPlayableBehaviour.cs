using GamePlay.Timeline.PlayableAsset.PlayableBehaviour;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Playables;
using UnityEngine.Animations;
namespace AngelBeat
{
    public class AnimationPlayableBehaviour: SkillTimeLinePlayableBehaviour
    {
        public Animator animator { get; set; }
        public AnimationClip animationClip { get; set; }

        PlayableGraph playableGraph;

        // Ŭ���� ���۵� �� ȣ��
        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            // PlayableGraph ����
            playableGraph = PlayableGraph.Create("AnimationPlayable");

            // AnimationClipPlayable ����
            AnimationClipPlayable clipPlayable = AnimationClipPlayable.Create(playableGraph, animationClip);

            // ��� ���� �� ����
            var output = AnimationPlayableOutput.Create(playableGraph, "Animation", animator);
            output.SetSourcePlayable(clipPlayable);

            // Playable ���
            playableGraph.Play();
        }

        // Ŭ���� ���� �� ȣ��
        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            if (playableGraph.IsValid())
            {
                playableGraph.Stop(); // �׷��� ����
                playableGraph.Destroy(); // �׷��� ���ҽ� ����
            }
            //animator.Play("IDLE",0,0);
        }
    }
}