using Cysharp.Threading.Tasks;
using GamePlay.Features.Battle.Scripts.BattleMap;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.Features.Battle.Scripts
{
    public sealed class BattleCameraDriver : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera followCam;
        [SerializeField] private float defaultBlendSeconds = 0.5f;
        [SerializeField] private float orthoSizeClampMax;
        [SerializeField] private bool useFocusZoom = true;
        [SerializeField] private float focusOrthoSize = 10f;
        [SerializeField] private float focusZoomSeconds = 0.35f;
        private CinemachineBrain _brain;


        private void Awake()
        {
            _brain = Camera.main ? Camera.main.GetComponent<CinemachineBrain>() : null;
            if (!_brain) Debug.LogWarning("[BattleCameraDriver] CinemachineBrain(MainCamera) 미발견");
        }
        
        public async UniTask ShowStageIntro(StageField stage, float paddingWorld = 1f, float fadeSeconds = 0.8f)
        {
            if (!stage || !followCam) return;

            var grid = stage.Grid;
            var lossy = stage.transform.lossyScale;
            Vector2 cellWorld = new(grid.cellSize.x * lossy.x, grid.cellSize.y * lossy.y);

            Vector2Int gs = stage.GridSize;
            Vector2 sizeWorld = new(gs.x * cellWorld.x, gs.y * cellWorld.y);

            Vector2Int half = gs / 2;
            var zeroCenter = (Vector2)grid.GetCellCenterWorld(Vector3Int.zero);
            Vector2 originWorld = zeroCenter - new Vector2(half.x + 0.5f, half.y + 0.5f) * cellWorld;

            Vector2 centerWorld = originWorld + 0.5f * sizeWorld;

            var lens = followCam.Lens;
            if (lens.Orthographic)
            {
                float need = ComputeOrthoSizeToFit(sizeWorld, paddingWorld);
                if (orthoSizeClampMax > 0f) need = Mathf.Min(need, orthoSizeClampMax);
                lens.OrthographicSize = need;
                followCam.Lens = lens;
            }
            else
            {
                Debug.LogWarning("[BattleCameraDriver] 현재 카메라가 Perspective입니다. " +
                                 "URP 스택상 Base/Overlay 투영 일치 권장. 런타임 강제 전환은 금지합니다.");
            }

            var anchor = EnsureStageAnchor(stage.transform, centerWorld);
            followCam.Follow = anchor;
            followCam.LookAt = anchor;
            followCam.Priority = 100;

            await FadeFromBlack(fadeSeconds);
        }

        private float ComputeOrthoSizeToFit(Vector2 sizeWorld, float pad)
        {
            var cam = _brain && _brain.OutputCamera ? _brain.OutputCamera : Camera.main;
            float aspect = cam ? cam.aspect : 16f / 9f;
            float halfH = sizeWorld.y * 0.5f;
            float halfW = sizeWorld.x * 0.5f;
            return Mathf.Max(halfH, halfW / Mathf.Max(0.0001f, aspect)) + pad;
        }

        private Transform EnsureStageAnchor(Transform stageRoot, Vector2 centerWorld)
        {
            const string Name = "__StageCameraAnchor";
            var t = stageRoot.Find(Name);
            if (!t)
            {
                var go = new GameObject(Name);
                go.transform.SetParent(stageRoot, worldPositionStays: false);
                t = go.transform;
            }
            t.position = centerWorld;
            t.rotation = Quaternion.identity;
            t.localScale = Vector3.one;
            return t;
        }

        private async UniTask FadeFromBlack(float dur)
        {
            if (dur <= 0f) return;
            var go = new GameObject("__BattleIntroFade");
            var canvas = go.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 10000;
            var cg = go.AddComponent<CanvasGroup>(); var img = go.AddComponent<Image>(); img.color = Color.black; cg.alpha = 1f;
            float t = 0f;
            while (t < dur) { t += Time.deltaTime; cg.alpha = Mathf.Lerp(1f, 0f, t / dur); await UniTask.Yield(); }
            Destroy(go);
        }
        
        public async UniTask Focus(Transform target,
                               float? blendSeconds = null,
                               float? targetOrthoSize = null,
                               float? zoomSeconds = null)
        {
            if (!target || !followCam || !_brain) return;

            followCam.Follow = target;
            followCam.LookAt = target;
            followCam.Priority = 100;

            var def = _brain.DefaultBlend;
            float prevBlend = def.Time;
            def.Time = blendSeconds ?? prevBlend;
            _brain.DefaultBlend = def;

            UniTask zoomTask = UniTask.CompletedTask;
            var lens = followCam.Lens;
            bool doZoom = (useFocusZoom || targetOrthoSize.HasValue) && lens.Orthographic;

            if (doZoom)
            {
                float aim = targetOrthoSize ?? focusOrthoSize;
                float dur = zoomSeconds ?? blendSeconds ?? focusZoomSeconds;
                zoomTask = ZoomTo(aim, dur);
            }

            var blendWait = UniTask.WaitUntil(() => !_brain.IsBlending);
            await UniTask.WhenAll(blendWait, zoomTask);

            def = _brain.DefaultBlend;
            def.Time = prevBlend;
            _brain.DefaultBlend = def;
        }

        /// <summary>
        /// OrthoSize(또는 FOV)를 duration 동안 부드럽게 보간
        /// </summary>
        public async UniTask ZoomTo(float targetValue, float duration)
        {
            if (!followCam) return;

            var lens = followCam.Lens;
            bool isOrtho = lens.Orthographic;
            float start = isOrtho ? lens.OrthographicSize : lens.FieldOfView;
            if (Mathf.Approximately(start, targetValue) || duration <= 0f)
            {
                if (isOrtho) { lens.OrthographicSize = targetValue; }
                else         { lens.FieldOfView      = targetValue; }
                followCam.Lens = lens;
                return;
            }

            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float w = Mathf.Clamp01(t / duration);
                // Smooth Step이라고 하네요
                float k = w * w * (3f - 2f * w);
                float v = Mathf.Lerp(start, targetValue, k);

                lens = followCam.Lens;
                if (isOrtho) { lens.OrthographicSize = v; }
                else         { lens.FieldOfView      = v; }
                followCam.Lens = lens;

                await UniTask.Yield();
            }
        }
            
        public void FocusImmediate(Transform target)
        {
            if (!target || !followCam) return;
            followCam.Follow = target;
            followCam.LookAt = target;
            followCam.Priority = 100;
        }
        
       
    }
    
    
}