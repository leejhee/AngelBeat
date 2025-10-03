using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// === 프로젝트 네임스페이스 (필요시 네 프로젝트에 맞게 수정) ===
using Core.Scripts.Foundation.Define;
using GamePlay.Common.Scripts.Entities.Skills; // SystemEnum.ePivot
using GamePlay.Features.Battle.Scripts;               // BattleController
using GamePlay.Features.Battle.Scripts.Unit;          // CharBase
using GamePlay.Common.Scripts.Skill;                  // SkillModel, SkillParameter (있는 경우)

namespace GamePlay.Common.Scripts.Skill.Preview
{
    /// <summary>
    /// - 셀(Indicator) 위에 커서가 올라가기만 하면, 셀 영역 안의 '유효 타겟' 캐릭터에 아웃라인을 입혀 강조한다.
    /// - 포인터를 빼면 아웃라인 해제.
    /// - 클릭 시 실제 시전은 옵션(clickCastsSkill)로 둠. (기본 꺼짐 = 호버 전용)
    /// - 캐릭터에 Collider2D가 없어도 SpriteRenderer.bounds로 폴백 탐색한다.
    /// - 아웃라인은 캐릭터의 자식으로 'OutlineOverlay' SpriteRenderer를 만들어 그린다(머티리얼 스왑 X).
    /// 사용 준비물:
    ///   1) 메인 카메라: Physics2DRaycaster, 씬에 EventSystem(InputSystemUIInputModule)
    ///   2) Indicator 프리팹: SpriteRenderer + Collider2D(isTrigger OK) + 본 스크립트
    ///   3) 캐릭터: CharBase + 자식에 SpriteRenderer(아웃라인 표시 대상)
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class SkillIndicator : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerMoveHandler
    {
        // ---------------- Outline Overlay ----------------
        [Header("Outline (Overlay Renderer)")]
        [Tooltip("외곽선에 사용할 머티리얼 (SpriteOutline.shader 등)")]
        [SerializeField] private Material outlineMaterial;
        [Tooltip("아웃라인 색 (셰이더에 _OutlineColor가 있을 때 적용)")]
        [SerializeField] private Color outlineColor = Color.yellow;
        [Tooltip("아웃라인 두께 (셰이더에 _OutlineSize가 있을 때 적용)")]
        [SerializeField] private float outlineSize = 1.5f;
        [Tooltip("본체 SpriteRenderer보다 몇 단계 위에 그릴지(+면 앞)")]
        [SerializeField] private int outlineSortingOffset = +1;

        // ---------------- Detect Targets ----------------
        [Header("Target Detection")]
        [Tooltip("셀 영역 탐색 크기(월드 유닛). autoProbeFromScale= true면 무시되고 자동으로 설정됨")]
        [SerializeField] private Vector2 probeSize = new(0.9f, 0.9f);
        [Tooltip("프리팹/셀 스케일로부터 probeSize 자동 계산")]
        [SerializeField] private bool autoProbeFromScale = true;
        [Tooltip("Collider 기반 물리 탐색을 우선 사용")]
        [SerializeField] private bool preferPhysicsOverlap = true;
        [Tooltip("캐릭터 탐색 레이어(비워두면 전 레이어)")]
        [SerializeField] private LayerMask characterMask = 0;
        [Tooltip("막힌 셀이라도 호버하면 아웃라인은 보여줄지")]
        [SerializeField] private bool outlineEvenIfBlocked = true;
        [Tooltip("디버그 로그")]
        [SerializeField] private bool debugLog = false;

        // ---------------- Hover Tint ----------------
        [Header("Hover Tint (Indicator 자체색 보정)")]
        [Tooltip("호버 시 밝기 배율(1 보다 작으면 진해짐)")]
        [SerializeField] private float hoverBrightnessMul = 0.90f;
        [Tooltip("호버 시 알파 추가량")]
        [SerializeField] private float hoverAlphaAdd = 0.05f;

        // ---------------- Optional Cast ----------------
        [Header("Cast (Optional)")]
        [Tooltip("클릭 시 실제 스킬 시전할지 여부 (기본:false)")]
        [SerializeField] private bool clickCastsSkill = false;

        // --------------- Runtime Injection ---------------
        private CharBase _caster;
        private SkillModel _skill;
        private bool _isBlocked;
        private SystemEnum.ePivot _pivotType = SystemEnum.ePivot.TARGET_ENEMY;
        private int _pointerRange = 0; // 0=단일

        // --------------- State ---------------
        private CharBase _hoverTarget;
        private SpriteRenderer _hoverSR;        // 대상 캐릭터의 SR
        private SpriteRenderer _outlineOverlay; // 외곽선 전용 SR
        private SpriteRenderer _cellSR;         // 내 칸 SR
        private Color _baseColor;
        private bool _hovered;

        // ---------------- Public API ----------------
        /// <summary>BattleController에서 instantiate 직후 호출</summary>
        public void Init(CharBase caster, SkillModel skill, bool isBlocked,
                         SystemEnum.ePivot pivotType = SystemEnum.ePivot.TARGET_ENEMY,
                         int pointerRange = 0)
        {
            _caster = caster;
            _skill = skill;
            _isBlocked = isBlocked;
            _pivotType = pivotType;
            _pointerRange = Mathf.Max(0, pointerRange);
        }

        // ---------------- Unity ----------------
        private void Awake()
        {
            _cellSR = GetComponent<SpriteRenderer>();
            if (_cellSR != null) _baseColor = _cellSR.color;

            if (autoProbeFromScale)
            {
                var s = transform.lossyScale;
                probeSize = new Vector2(Mathf.Abs(s.x), Mathf.Abs(s.y)) * 0.98f; // 모서리 여유
            }

            if (characterMask == 0)
            {
                int ch = LayerMask.NameToLayer("Character");
                if (ch >= 0) characterMask = 1 << ch; // 없으면 전 레이어 검색
            }
        }

        private void OnEnable()
        {
            // 프리뷰가 켜진 프레임에 이미 커서가 위라면 즉시 반응
            if (IsPointerCurrentlyOverThis())
            {
                ApplyHoverTint(true);
                TryOutlineTargetInCell();
            }
        }

        private void OnDisable()  { ClearOutline(); ApplyHoverTint(false); }
        private void OnDestroy()  { ClearOutline(); }

        // ---------------- Event Interfaces ----------------
        public void OnPointerEnter(PointerEventData eventData)
        {
            ApplyHoverTint(true);
            if (_isBlocked && !outlineEvenIfBlocked) return;
            TryOutlineTargetInCell();
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            if (_isBlocked && !outlineEvenIfBlocked) return;
            TryOutlineTargetInCell();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ClearOutline();
            ApplyHoverTint(false);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!clickCastsSkill) return;
            if (_isBlocked) return;

            // 클릭 시 다시 타겟 확인
            if (!TryOutlineTargetInCell()) return;
            if (!IsValidTarget(_hoverTarget)) return;

            var targets = new List<CharBase> { _hoverTarget };
            // 범위 타겟팅(필요 시)
            if (_pointerRange > 0)
            {
                targets.Clear();
                Vector2 center = (_pivotType == SystemEnum.ePivot.TARGET_SELF)
                    ? (Vector2)_caster.CharTransform.position
                    : (Vector2)transform.position;

                Vector2 box = new(_pointerRange, 1f);
                var cols = Physics2D.OverlapBoxAll(center, box, 0f, characterMask);
                foreach (var c in cols)
                    if (c && c.TryGetComponent(out CharBase cb) && cb != null && IsValidTarget(cb))
                        targets.Add(cb);
            }

            if (targets.Count == 0) return;

            // 실제 시전 (네 프로젝트 시그니처에 맞게 조절)
            if (_caster != null && _skill != null)
            {
                _caster.SkillInfo?.PlaySkill(
                    _skill.SkillIndex,
                    new SkillParameter(_caster, targets, _skill)
                );
            }

            BattleController.Instance?.HideSkillPreview();
            ClearOutline();
        }

        // ---------------- Core Logic ----------------
        private bool TryOutlineTargetInCell()
        {
            ClearOutline();

            CharBase best = FindBestTargetInCell();
            if (best == null) return false;
            if (!IsValidTarget(best)) return false;

            _hoverTarget = best;

            // 대상의 SpriteRenderer 가져오기
            var root = _hoverTarget.CharUnitRoot;
            if (!root || !root.TryGetComponent(out _hoverSR))
                _hoverSR = _hoverTarget.GetComponentInChildren<SpriteRenderer>();

            if (_hoverSR == null || outlineMaterial == null) return false;

            EnsureOverlay(_hoverSR);
            _outlineOverlay.sprite = _hoverSR.sprite;
            _outlineOverlay.enabled = true;

            return true;
        }

        private void ClearOutline()
        {
            if (_outlineOverlay != null) _outlineOverlay.enabled = false;
            _hoverSR = null;
            _hoverTarget = null;
        }

        private CharBase FindBestTargetInCell()
        {
            Bounds cell = new Bounds(transform.position, new Vector3(probeSize.x, probeSize.y, 10f));
            CharBase best = null; int bestOrder = int.MinValue;

            if (preferPhysicsOverlap)
            {
                int mask = (characterMask.value == 0) ? ~0 : characterMask.value;
                var cols = Physics2D.OverlapBoxAll(cell.center, new Vector2(cell.size.x, cell.size.y), 0f, mask);
                if (debugLog) Debug.Log($"[Indicator] physics hits={cols.Length}");
                foreach (var c in cols)
                {
                    if (!c) continue;
                    var cb = c.GetComponentInParent<CharBase>();
                    if (cb == null) continue;
                    var sr = cb.GetComponentInChildren<SpriteRenderer>();
                    int so = sr ? sr.sortingOrder : 0;
                    if (best == null || so > bestOrder) { best = cb; bestOrder = so; }
                }
                if (best != null) return best;
            }

            // 폴백: CharBase 전체 스캔 + Renderer.bounds 교차
            var all = FindObjectsOfType<CharBase>();
            if (debugLog) Debug.Log($"[Indicator] fallback scan count={all.Length}");
            foreach (var cb in all)
            {
                var sr = cb.GetComponentInChildren<SpriteRenderer>();
                if (sr == null) continue;
                if (!sr.bounds.Intersects(cell)) continue;
                int so = sr.sortingOrder;
                if (best == null || so > bestOrder) { best = cb; bestOrder = so; }
            }
            return best;
        }

        private bool IsValidTarget(CharBase target)
        {
            if (target == null) return false;

            switch (_pivotType)
            {
                case SystemEnum.ePivot.TARGET_SELF:   return target == _caster;
                case SystemEnum.ePivot.TARGET_ENEMY:  return target != _caster; // 필요 시 팀/진영 비교 로직으로 교체
                default:                              return true;
            }
        }

        // ---------------- Helpers ----------------
        private void EnsureOverlay(SpriteRenderer target)
        {
            // 이미 같은 타겟에 연결되어 있으면 재사용
            if (_outlineOverlay != null && _outlineOverlay.transform.parent == target.transform)
            {
                ApplyOutlineMaterialProps(_outlineOverlay);
                _outlineOverlay.sortingLayerID = target.sortingLayerID;
                _outlineOverlay.sortingOrder = target.sortingOrder + outlineSortingOffset;
                return;
            }

            // 기존 것 끄기
            if (_outlineOverlay != null) _outlineOverlay.enabled = false;

            // 새 오버레이 생성
            var go = new GameObject("OutlineOverlay");
            go.transform.SetParent(target.transform, false);
            _outlineOverlay = go.AddComponent<SpriteRenderer>();
            _outlineOverlay.material = outlineMaterial;
            ApplyOutlineMaterialProps(_outlineOverlay);
            _outlineOverlay.sortingLayerID = target.sortingLayerID;
            _outlineOverlay.sortingOrder = target.sortingOrder + outlineSortingOffset;
            _outlineOverlay.color = Color.white;
            _outlineOverlay.enabled = false;
        }

        private void ApplyOutlineMaterialProps(SpriteRenderer sr)
        {
            if (sr == null || sr.material == null) return;
            if (sr.material.HasProperty("_OutlineColor")) sr.material.SetColor("_OutlineColor", outlineColor);
            if (sr.material.HasProperty("_OutlineSize"))  sr.material.SetFloat("_OutlineSize", outlineSize);
        }

        private void ApplyHoverTint(bool on)
        {
            if (_cellSR == null) return;
            if (on)
            {
                if (!_hovered) { _baseColor = _cellSR.color; _hovered = true; }
                var c = _baseColor;
                c.r = Mathf.Clamp01(c.r * hoverBrightnessMul);
                c.g = Mathf.Clamp01(c.g * hoverBrightnessMul);
                c.b = Mathf.Clamp01(c.b * hoverBrightnessMul);
                c.a = Mathf.Clamp01(c.a + hoverAlphaAdd);
                _cellSR.color = c;
            }
            else
            {
                if (_hovered) { _cellSR.color = _baseColor; _hovered = false; }
            }
        }

        private bool IsPointerCurrentlyOverThis()
        {
            if (EventSystem.current == null) return false;

            Vector2 pos;
            if (Mouse.current != null) pos = Mouse.current.position.ReadValue();
            else pos = Input.mousePosition;

            var ed = new PointerEventData(EventSystem.current) { position = pos };
            var results = new List<RaycastResult>(8);
            EventSystem.current.RaycastAll(ed, results);
            foreach (var r in results)
                if (r.gameObject == gameObject) return true;
            return false;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, new Vector3(probeSize.x, probeSize.y, 0.1f));
        }
#endif
    }
}
