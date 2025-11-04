using GamePlay.Features.Battle.Scripts.Unit;
using System;
using UnityEngine;


namespace GamePlay.Features.Battle.Scripts.UI.BattleHovering
{
    [ExecuteAlways]
    public class CharacterHpBar : MonoBehaviour
    {
        [SerializeField] private CharBase charBase;
        [SerializeField] private SpriteRenderer hpBar;
        private MaterialPropertyBlock _mp = new();

        [Range(0f, 1f)]
        public float fillAmount = 1f;
        
        private void Awake()
        {
            // Awake에서 한 번 초기화
            if (_mp == null)
                _mp = new MaterialPropertyBlock();
        }

        public void SetFillAmount(float now, float max)
        {
            if (_mp == null)
                _mp = new MaterialPropertyBlock();

            if (hpBar == null)
                return;
            hpBar.GetPropertyBlock(_mp);
            _mp.SetFloat("_Fill", Mathf.Clamp01(now/max));
            hpBar.SetPropertyBlock(_mp);
        }

        public void SetFillAmount(float amount)
        {
            if (_mp == null)
                _mp = new MaterialPropertyBlock();

            if (hpBar == null)
                return;
            hpBar.GetPropertyBlock(_mp);
            _mp.SetFloat("_Fill", Mathf.Clamp01(amount));
            hpBar.SetPropertyBlock(_mp);
        }
        private void Update()
        {
            #if UNITY_EDITOR
            // 에디터에서만 자동 갱신
            if (!Application.isPlaying)
                SetFillAmount(fillAmount);
            #endif
        }
    }
}
