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
        private MaterialPropertyBlock _mp;

        [Range(0f, 1f)]
        public float fillAmount = 1f;
        
        private void Start()
        {
            hpBar.GetPropertyBlock(_mp);
            _mp = new MaterialPropertyBlock(); ;
        }

        public void SetFillAmount(float now, float max)
        {
            float amount = now / max;
            _mp.SetFloat("_Fill", amount);
            hpBar.SetPropertyBlock(_mp);
        }

        public void SetFillAmount(float amount)
        {
            _mp.SetFloat("_Fill", amount);
            hpBar.SetPropertyBlock(_mp);
        }
        private void Update()
        {
            SetFillAmount(fillAmount);
        }
    }
}
