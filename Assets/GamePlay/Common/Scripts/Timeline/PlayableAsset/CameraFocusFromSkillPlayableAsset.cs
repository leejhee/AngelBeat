// CameraFocusFromSkillPlayableAsset.cs
using System;
using UnityEngine;
using UnityEngine.Playables;
using GamePlay.Common.Scripts.Timeline.PlayableAsset;
using GamePlay.Common.Scripts.Timeline.PlayableBehaviour;

public enum SkillCameraTargetRole { Caster, Target }
public enum SkillCameraEffectType { FocusZoom /*, Shake, Group ...*/ }

[Serializable]
public sealed class CameraFocusFromSkillPlayableAsset : SkillTimeLinePlayableAsset
{
    [Header("Target")]
    public SkillCameraTargetRole role = SkillCameraTargetRole.Caster;

    [Header("Effect")]
    public SkillCameraEffectType effect = SkillCameraEffectType.FocusZoom;
    public bool setFollow = true;
    public bool disableInputWhilePlaying = true;
    public bool restoreZoomOnEnd = false;

    [Header("Zoom (FocusZoom)")]
    public bool animateZoom = true;
    public float targetOrthoSize = 10f;
    public AnimationCurve ease = AnimationCurve.EaseInOut(0,0,1,1);

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = CreatePlayableWithContext<CameraFocusFromSkillPlayableBehaviour>(graph, owner);
        var bhv = playable.GetBehaviour();

        // 컨텍스트가 없으면 no-op
        if (!bhv.HasContext) return playable;

        // Behaviour에 파라미터 전달
        bhv.role = role;
        bhv.effect = effect;
        bhv.setFollow = setFollow;
        bhv.disableInputWhilePlaying = disableInputWhilePlaying;
        bhv.restoreZoomOnEnd = restoreZoomOnEnd;

        bhv.animateZoom = animateZoom;
        bhv.targetOrthoSize = targetOrthoSize;
        bhv.ease = ease;

        return playable;
    }
}