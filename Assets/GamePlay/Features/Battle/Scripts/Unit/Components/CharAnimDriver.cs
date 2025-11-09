using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace GamePlay.Features.Battle.Scripts.Unit.Components
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    public sealed class CharAnimDriver : MonoBehaviour
    {
        // ===== 통일된 파라미터 이름들 (모든 Animator에 동일하게 만들어 둘 것) =====
        // - Animator Parameters:
        //   Bool:  IsMoving, OnAttack, Push, Evade, JumpOut, JumpIn
        //   Float: Speed   (선택; BlendTree 쓰는 경우)
        private static readonly int P_IsMoving = Animator.StringToHash("IsMoving");
        private static readonly int P_OnAttack = Animator.StringToHash("OnAttack");
        private static readonly int P_Push = Animator.StringToHash("Push");
        private static readonly int P_Evade = Animator.StringToHash("Evade");
        private static readonly int P_JumpOut = Animator.StringToHash("JumpOut");
        private static readonly int P_JumpIn = Animator.StringToHash("JumpIn");
        //private static readonly int P_Speed = Animator.StringToHash("Speed");

        [SerializeField] private Animator animator; // 비워두면 Awake에서 GetComponent
        public Animator Animator => animator;

        // 타임라인 임대/반납 (Director가 잡는 동안은 파라미터 구동 금지)
        private int _timelineLease;
        public bool IsTimelineActive => _timelineLease > 0;

        private sealed class Lease : IDisposable
        {
            private readonly CharAnimDriver _d;
            public Lease(CharAnimDriver d) { _d = d; }

            public void Dispose()
            {
                if (_d._timelineLease > 0) _d._timelineLease--;
                if (_d._timelineLease == 0 && _d.animator)
                {
                    _d.animator.Rebind();
                    _d.animator.Update(0f);
                }
            }
        }

        public IDisposable AcquireTimelineLease()
        {
            _timelineLease++;
            return new Lease(this);
        }

        // 파라미터 존재 캐시(경고 1회만)
        private HashSet<int> _availableParams;
        private readonly HashSet<int> _warnedMissing = new();

        // 내부 토큰 (상태 유지 중 중단 시 최신 것만 유효)
        private CancellationTokenSource _cts;

        private CancellationToken NewToken(CancellationToken external = default)
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = CancellationTokenSource.CreateLinkedTokenSource(external);
            return _cts.Token;
        }

        void Awake()
        {
            if (!animator) animator = GetComponent<Animator>();
            BuildParamCache();
        }

        void OnDisable()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        void BuildParamCache()
        {
            _availableParams = new HashSet<int>();
            if (!animator) return;
            foreach (var p in animator.parameters)
                _availableParams.Add(p.nameHash);
        }

        bool CanDrive => !IsTimelineActive && animator && animator.isActiveAndEnabled;

        void SetBoolParam(int id, bool value)
        {
            if (!CanDrive) return;
            if (!_availableParams.Contains(id))
            {
                if (_warnedMissing.Add(id))
                    Debug.LogWarning(
                        $"[AnimDriver] Animator '{animator.name}' missing bool param hash {id}. Check controller params.");
                return;
            }

            animator.SetBool(id, value);
        }

        void SetFloatParam(int id, float value)
        {
            if (!CanDrive) return;
            if (!_availableParams.Contains(id))
            {
                if (_warnedMissing.Add(id))
                    Debug.LogWarning(
                        $"[AnimDriver] Animator '{animator.name}' missing float param hash {id}. Check controller params.");
                return;
            }

            animator.SetFloat(id, value);
        }

        // ===== 공개 API (편의 메서드) =====
        public void SetMoving(bool v) => SetBoolParam(P_IsMoving, v);
        public void SetOnAttack(bool v) => SetBoolParam(P_OnAttack, v);
        public void SetPush(bool v) => SetBoolParam(P_Push, v);
        public void SetEvade(bool v) => SetBoolParam(P_Evade, v);
        public void SetJumpOut(bool v) => SetBoolParam(P_JumpOut, v);
        public void SetJumpIn(bool v) => SetBoolParam(P_JumpIn, v);
        //public void SetSpeed(float v) => SetFloatParam(P_Speed, v);

        /// body 실행 동안 특정 Bool을 true로 유지 → 종료 시 false
        public async UniTask WithFlag(int paramId, Func<CancellationToken, UniTask> body, CancellationToken ct = default)
        {
            ct = NewToken(ct);
            SetBoolParam(paramId, true);
            try { await body(ct); }
            finally { SetBoolParam(paramId, false); }
        }

        // 전용 스코프 편의 함수들
        public UniTask WithMoving(Func<CancellationToken, UniTask> body, CancellationToken ct = default)
            => WithFlag(P_IsMoving, body, ct);

        public UniTask WithOnAttack(Func<CancellationToken, UniTask> body, CancellationToken ct = default)
            => WithFlag(P_OnAttack, body, ct);

        public UniTask WithPush(Func<CancellationToken, UniTask> body, CancellationToken ct = default)
            => WithFlag(P_Push, body, ct);

        public UniTask WithEvade(Func<CancellationToken, UniTask> body, CancellationToken ct = default)
            => WithFlag(P_Evade, body, ct);

        public UniTask WithJumpOut(Func<CancellationToken, UniTask> body, CancellationToken ct = default)
            => WithFlag(P_JumpOut, body, ct);

        public UniTask WithJumpIn(Func<CancellationToken, UniTask> body, CancellationToken ct = default)
            => WithFlag(P_JumpIn, body, ct);

        /// 모든 플래그/스피드 초기화
        public void ResetAllFlags()
        {
            SetMoving(false);
            SetOnAttack(false);
            SetPush(false);
            SetEvade(false);
            SetJumpOut(false);
            SetJumpIn(false);
            //SetSpeed(0f);
        }
    }
}
