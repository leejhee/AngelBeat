using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Playables;
using UnityEngine.Animations;
namespace Client
{
    public class AnimationPlayableBehaviour: SkillTimeLinePlayableBehaviour
    {
        public Animator animator { get; set; }
        public AnimationClip animationClip { get; set; }

        PlayableGraph playableGraph;

        // 클립이 시작될 때 호출
        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            // PlayableGraph 생성
            playableGraph = PlayableGraph.Create("AnimationPlayable");

            // AnimationClipPlayable 생성
            AnimationClipPlayable clipPlayable = AnimationClipPlayable.Create(playableGraph, animationClip);

            // 출력 생성 및 연결
            var output = AnimationPlayableOutput.Create(playableGraph, "Animation", animator);
            output.SetSourcePlayable(clipPlayable);

            // Playable 재생
            playableGraph.Play();
        }

        // 클립이 멈출 때 호출
        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            if (playableGraph.IsValid())
            {
                playableGraph.Stop(); // 그래프 정지
                playableGraph.Destroy(); // 그래프 리소스 해제
            }
            animator.Play("IDLE",0,0);
        }
    }
}