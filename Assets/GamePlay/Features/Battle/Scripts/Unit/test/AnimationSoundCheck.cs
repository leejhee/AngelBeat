using Core.Scripts.Foundation.Define;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationSoundCheck : MonoBehaviour
{
    private static readonly int SmokeStepJumpIn = Animator.StringToHash("SmokeStepJumpIn");
    public AudioSource audioSource;
    public AudioClip audioClip;
    public Animator animator;
    public YeonSkill skill;
    public void PlaySound()
    {
        audioSource.PlayOneShot(audioClip);
    }

    public void ToIdle()
    {
        animator.SetTrigger("Idle");
    }
    public void PlayAnimation()
    {
        animator.SetTrigger(skill.ToString());
    }

    public void PlayAnim(YeonSkill yeonSkill)
    {
        animator.SetTrigger(yeonSkill.ToString());
    }
    public void PlayJumpInTrigger()
    {
        animator.SetTrigger(SmokeStepJumpIn);
    }
    
    public void PlayAudioClip(AudioClip sound)
    {
        audioSource.PlayOneShot(sound);
    }
    
    
    
    public enum YeonSkill
    {
        MungeCloud,
        Twister,
        SmokeWave,
        SmokeStepCast,
        SmokeStepJumpIn,
        SmokeStepJumpOut,
        SmokeBind,
        SmokeBindEnd
    }
}
