using Cysharp.Threading.Tasks;
using GamePlay.Common.Scripts.Contracts;
using GamePlay.Features.Battle.Scripts.BattleMap;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

namespace GamePlay.Features.Battle.Scripts.Unit
{
    public class FieldCover : MonoBehaviour, IDamageable, IPointerEnterHandler, IPointerExitHandler
    {
        private const float DamageProbability = 50;

        public long NMHP;
        public long NHP;

        private BattleStageGrid _grid;
        private Vector2Int _cell;
        public event Action<FieldCover> Broken;
        
        private void Start()
        {
            NHP = NMHP;
        }

        public void BindGrid(BattleStageGrid grid, Vector2Int cell)
        {
            _grid = grid;
            _cell = cell;
        }

        public async UniTask DamageAsync(long _, CancellationToken ct)
        {
            bool hit = Random.Range(0f, 100f) <= DamageProbability;
            if (hit)
            {
                //대미지 폰트
                NHP --;
                if (NHP <= 0)
                {
                    Broken?.Invoke(this);
                    Destroy(gameObject);
                }
            }
            else
            {
                //감나빗 효과
            }
        }

        private void OnDestroy()
        {
            _grid.UnregisterCover(this);
        }

        /// <summary>
        /// 올렸을 때 장애물 타수 정보가 나와야 함.
        /// </summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            // 텍스트 박스 + 몇대 남았는지
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // 텍스트 박스 치워야지
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out CharBase character)) return;
            character.RegisterCoverage(this);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.TryGetComponent(out CharBase character)) return;
            character.ExitCoverage();
        }
    }
}