using GamePlay.Features.Battle.Scripts.BattleMap;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GamePlay.Features.Battle.Scripts
{
    public class BattleCameraInput : MonoBehaviour
    {
        [Header("Refs")]
        public BattleCameraDriver driver;
        public StageField stage;

        [Header("Input Actions (InputActionReference)")]
        public InputActionReference panButton;   // Button: <Mouse>/middleButton (or rightButton)
        public InputActionReference pan;         // Vector2: <Pointer>/delta
        public InputActionReference zoom;        // Vector2: <Mouse>/scroll

        [Header("Tuning")]
        public float dragSensitivity = 1.0f;     // 드래그 속도
        public float wheelStep      = 1.0f;      // 줌 1칸당 OrthoSize 변화량
        public bool  enableDuringTurn = true;
        
        [Header("Wheel Tuning")]
        [SerializeField] private bool  normalizeWheel   = true;  // OS/장치 차이를 줄이기
        [SerializeField] private float wheelDetent      = 120f;  // Windows 기본 한 칸(=120)
        [SerializeField] private bool  multiplicative   = true;  // 비율(권장) vs 선형
        [SerializeField] private float zoomMulPerStep   = 0.9f;  // 한 칸마다 0.9배 (줌 인). 0.95~0.98도 무난
        [SerializeField] private float zoomLinearStep   = 0.5f;  // 선형일 때 한 칸당 사이즈 변화량
        
        private Camera _cam;
        private bool _panning;

        private void OnEnable()
        {
            _cam ??= Camera.main;

            if (panButton) { panButton.action.Enable(); panButton.action.performed += OnPanPressed; panButton.action.canceled += OnPanReleased; }
            if (pan)       { pan.action.Enable(); }
            if (zoom)      { zoom.action.Enable(); }
        }

        private void OnDisable()
        {
            if (panButton) { panButton.action.performed -= OnPanPressed; panButton.action.canceled -= OnPanReleased; panButton.action.Disable(); }
            if (pan)       { pan.action.Disable(); }
            if (zoom)      { zoom.action.Disable(); }
        }

        private void Update()
        {
            if (!enableDuringTurn || !driver || !stage) return;
            _cam ??= Camera.main;

            // --- Zoom ---
            if (zoom)
            {
                float raw = zoom.action.ReadValue<Vector2>().y;
                if (Mathf.Abs(raw) > 0.01f && driver.IsOrtho)
                {
                    // 1) OS/디바이스 차이 정규화
                    float steps = normalizeWheel ? (raw / Mathf.Max(1f, wheelDetent)) : raw;

                    float cur = driver.CurrentOrthoSize;
                    float next;

                    // 2) 비율/선형 중 택1
                    if (multiplicative)
                    {
                        // steps>0 (위로 스크롤)일 때 0.9^steps → 사이즈 감소(줌 인)
                        next = cur * Mathf.Pow(zoomMulPerStep, steps);
                    }
                    else
                    {
                        // 선형: 한 칸당 zoomLinearStep 만큼 변화
                        next = cur - steps * zoomLinearStep;
                    }

                    driver.SetZoom(next);
                }
            }

            // --- Pan ---
            if (_panning && pan)
            {
                Vector2 pxDelta = pan.action.ReadValue<Vector2>(); // 픽셀 단위
                if (pxDelta.sqrMagnitude > 0.0001f && driver.IsOrtho)
                {
                    float worldPerPixel = WorldUnitsPerPixel();
                    Vector2 worldDelta = -pxDelta * worldPerPixel * dragSensitivity;
                    driver.PanFree(worldDelta);
                }
            }
        }

        public void Bind(StageField s, BattleCameraDriver d = null)
        {
            stage = s;
            if (!driver)
            {
#if UNITY_2023_1_OR_NEWER
                driver = d ? d : Object.FindFirstObjectByType<BattleCameraDriver>(FindObjectsInactive.Exclude);
#else
                driver = d ? d : FindObjectOfType<BattleCameraDriver>();
#endif
            }
        }
        
        void OnPanPressed(InputAction.CallbackContext _)
        {
            if (!driver.IsLockedToTarget) { _panning = true; return; }
            // 처음 드래그 시작 시 Free 모드로 전환
            driver.EnterFree(stage);
            _panning = true;
        }

        void OnPanReleased(InputAction.CallbackContext _) => _panning = false;

        float WorldUnitsPerPixel()
        {
            if (!driver.IsOrtho) return 0.01f;
            _cam ??= Camera.main;
            float h = _cam ? _cam.pixelHeight : Screen.height;
            // orthoSize는 화면 세로의 절반에 해당
            return (driver.CurrentOrthoSize * 2f) / Mathf.Max(1f, h);
        }
    }
}