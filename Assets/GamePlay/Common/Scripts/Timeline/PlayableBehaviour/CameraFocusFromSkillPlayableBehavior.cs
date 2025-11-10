// CameraFocusFromSkillPlayableBehaviour.cs

using Cysharp.Threading.Tasks;
using GamePlay.Common.Scripts.Skill;
using GamePlay.Features.Battle.Scripts;
using GamePlay.Features.Battle.Scripts.Unit;
using UnityEngine;
using UnityEngine.Playables;

namespace GamePlay.Common.Scripts.Timeline.PlayableBehaviour
{
    public sealed class CameraFocusFromSkillPlayableBehaviour : SkillTimeLinePlayableBehaviour
    {
        // 에셋으로부터 전달되는 설정
        public SkillCameraTargetRole role;
        public SkillCameraEffectType effect;
        public bool setFollow;
        public bool disableInputWhilePlaying;
        public bool restoreZoomOnEnd;

        public bool animateZoom;
        public float targetOrthoSize;
        public AnimationCurve ease;

        // 내부 상태
        BattleCameraDriver _driver;
        BattleCameraInput _input;
        Transform _targetTF;
        float _prevOrtho;
        ICamEffectStrategy _strategy;
        bool _hadDriver;
        bool _hadInput;
        double _duration;

        public override void OnGraphStart(Playable playable)
        {
            _driver = BattleController.Instance.CameraDriver;
            _input  = Object.FindObjectOfType<BattleCameraInput>();
        
            _hadDriver = _driver != null;
            _hadInput  = _input  != null;

            if (_hadDriver && _driver.IsOrtho)
                _prevOrtho = _driver.CurrentOrthoSize;
        }

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            if (!HasContext || !_hadDriver) return;

            // 1) 대상 해석
            _targetTF = ResolveTargetTransform(role, Char, Skill);

            // 2) 입력 잠금
            if (_hadInput && disableInputWhilePlaying)
                _input.enableDuringTurn = false;

            // 3) Follow 전환(브레인 블렌드 활용)
            if (setFollow && _targetTF)
                _driver.EnterFollowAsync(_targetTF).Forget();

            // 4) 전략 구성
            _duration = playable.GetDuration();
            _strategy = BuildStrategy();
            var ctx = MakeCtx();
            _strategy?.OnPlay(ctx);
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (!HasContext || !_hadDriver || _strategy == null) return;

            double dur = _duration > 0 ? _duration : playable.GetDuration();
            double time = playable.GetTime();
            float t01 = (dur > 0.0) ? Mathf.Clamp01((float)(time / dur)) : 1f;

            _strategy.Process(MakeCtx(), t01);
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            if (_strategy != null) _strategy.OnPause(MakeCtx());

            if (_hadDriver && restoreZoomOnEnd && _driver.IsOrtho)
                _driver.SetZoom(_prevOrtho);

            if (_hadInput && disableInputWhilePlaying)
                _input.enableDuringTurn = true;
        }

        public override void OnGraphStop(Playable playable)
        {
            if (_hadInput) _input.enableDuringTurn = true; // 안전핀
        }

        // --------- 내부 유틸 ---------
        CameraCtx MakeCtx() => new CameraCtx
        {
            driver = _driver,
            input  = _input,
            target = _targetTF,
            startOrtho = _prevOrtho,
            duration = (float)(_duration > 0 ? _duration : 0.001)
        };

        ICamEffectStrategy BuildStrategy()
        {
            switch (effect)
            {
                case SkillCameraEffectType.FocusZoom:
                    if (!animateZoom || !_driver.IsOrtho) return null;
                    return new FocusZoomStrategy(targetOrthoSize, ease);
                default:
                    return null;
            }
        }

        static Transform ResolveTargetTransform(SkillCameraTargetRole role, CharBase caster, SkillBase skill)
        {
            CharBase who = (role == SkillCameraTargetRole.Caster) ? caster : FindPrimaryTarget(skill);
            if (!who) return null;
            return who.CharCameraPos ? who.CharCameraPos : who.transform;
        }

        // 스킬에서 "대표 타겟"을 추출(흔한 프로퍼티/필드 네이밍 검사)
        static CharBase FindPrimaryTarget(SkillBase skill)
        {
            if (!skill) return null;

            var targets = skill.SkillParameter.Target;
            if (targets == null || targets.Count == 0) return null;
            
            return targets[0];
        }

    }
}
