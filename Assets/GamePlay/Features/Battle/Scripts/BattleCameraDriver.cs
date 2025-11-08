using Cysharp.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;

namespace GamePlay.Features.Battle.Scripts
{
    public sealed class BattleCameraDriver : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera followCam;
        [SerializeField] private float defaultBlendSeconds = 0.5f;
        private CinemachineBrain _brain;


        private void Awake()
        {
            _brain = Camera.main ? Camera.main.GetComponent<CinemachineBrain>() : null;
            if (!_brain) Debug.LogWarning("[BattleCameraDriver] CinemachineBrain(MainCamera) 미발견");
        }

        public async UniTask Focus(Transform target, float? blendSeconds = null)
        {
            if (!target || !followCam || !_brain) return;

            followCam.Follow = target;
            followCam.LookAt = target;
            followCam.Priority = 100; // 보장

            var def = _brain.DefaultBlend;
            float prev = def.Time;
            def.Time = blendSeconds ?? defaultBlendSeconds;
            _brain.DefaultBlend = def;

            await UniTask.WaitUntil(() => !_brain.IsBlending);

            def = _brain.DefaultBlend;
            def.Time = prev;
            _brain.DefaultBlend = def;
        }
        
        public void FocusImmediate(Transform target)
        {
            if (!target || !followCam) return;
            followCam.Follow = target;
            followCam.LookAt = target;
            followCam.Priority = 100;
        }
        
        public async UniTask ZoomTo(float targetValue, float duration)
        {
            if (!followCam) return;

            LensSettings lens = followCam.Lens;
            bool isOrtho = lens.Orthographic;
            float start = isOrtho ? lens.OrthographicSize : lens.FieldOfView;

            if (duration <= 0f)
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
                float v = Mathf.Lerp(start, targetValue, w);

                lens = followCam.Lens; 
                if (isOrtho) { lens.OrthographicSize = v; }
                else         { lens.FieldOfView      = v; }
                followCam.Lens = lens;
                await UniTask.Yield();
            }
        }
    }
    
    
}