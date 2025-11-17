using Cysharp.Threading.Tasks;
using GamePlay.Features.Battle.Scripts.Unit;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.Features.Battle.Scripts.Tutorial
{
    public class BattleTutorialGuideUI : MonoBehaviour
    {
        public static BattleTutorialGuideUI Instance { get; private set; }
        
        [Header("공통 패널")]
        [SerializeField] private Canvas rootCanvas;
        [SerializeField] private RectTransform panel;
        [SerializeField] private TextMeshProUGUI guideText;
        [SerializeField] private Image arrowImage; // 있으면 사용, 없으면 무시
        
        [Header("조작 버튼")]
        [SerializeField] private Button nextButton;
        
        private Camera _cam;
        private UniTaskCompletionSource<bool> _waitTcs;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            _cam = Camera.main;
            Hide();

            if (nextButton != null)
                nextButton.onClick.AddListener(OnClickNext);
        }

        private void OnDestroy()
        {
            if (nextButton != null)
                nextButton.onClick.RemoveListener(OnClickNext);
        
            _waitTcs?.TrySetCanceled();
            _waitTcs = null;
        }

        private void OnClickNext()
        {
            _waitTcs?.TrySetResult(true);
        }
        
        public void ForceNext()
        {
            _waitTcs?.TrySetResult(true);
        }
        
        public async UniTask WaitForNextAsync()
        {
            _waitTcs = new UniTaskCompletionSource<bool>();
            await _waitTcs.Task;
            _waitTcs = null;
        }

        public void ShowScreenTop(string text)
        {
            if (!panel) return;
            if (guideText) guideText.text = text;

            panel.anchorMin = new Vector2(0.5f, 1f);
            panel.anchorMax = new Vector2(0.5f, 1f);
            panel.pivot = new Vector2(0.5f, 1f);
            panel.anchoredPosition = new Vector2(0f, -50f);

            panel.gameObject.SetActive(true);
        }

        public void ShowForActor(CharBase actor, string text)
        {
            if (!panel || !actor || _cam == null) return;
            if (guideText) guideText.text = text;

            Vector3 world = actor.CharCameraPos.position;
            Vector3 screen = _cam.WorldToScreenPoint(world);

            panel.anchorMin = new Vector2(0.5f, 0.5f);
            panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot = new Vector2(0.5f, 0f);

            panel.position = screen + new Vector3(0f, 80f, 0f);

            panel.gameObject.SetActive(true);
        }
        
        public void ShowForScreenPosition(Vector2 normalizedPos, string text)
        {
            if (!panel || !rootCanvas) return;
            if (guideText) guideText.text = text;

            RectTransform canvasRect = rootCanvas.transform as RectTransform;
            if (!canvasRect) return;

            float x = (normalizedPos.x - 0.5f) * canvasRect.rect.width;
            float y = (normalizedPos.y - 0.5f) * canvasRect.rect.height;

            panel.anchorMin = new Vector2(0.5f, 0.5f);
            panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot = new Vector2(0.5f, 0f);

            panel.anchoredPosition = new Vector2(x, y);

            panel.gameObject.SetActive(true);
        }

        public void Hide()
        {
            if (panel)
                panel.gameObject.SetActive(false);
            _waitTcs?.TrySetResult(false);
            _waitTcs = null;
        }
    }
}