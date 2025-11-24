using GamePlay.Common.Scripts.Entities.Character;
using GamePlay.Contracts.Interaction;
using System.Threading;
using UnityEngine;

namespace GamePlay.Features.Explore.Scripts
{
    /// <summary>
    /// '탐사' 씬에서 유저가 조종할 컨트롤러.
    /// </summary>
    public class ExploreController : MonoBehaviour, IInteractor
    {
        [SerializeField] private float speed;
        [SerializeField] private Party playerParty;
        public Party PlayerParty => playerParty;
        
        public Transform Transform { get; }
        public CancellationToken LifeTimeToken { get; }
        
        private void Start()
        {
            // playerParty = new Party();
            // Debug.Log($"{playerParty.SearchCharacter("샤오").Name}");
        }
        
        #region Interaction Part
        
        private IInteractable interactable;
        /// <summary>
        /// Input System 기반 메서드. Focus Object에 대한 상호작용
        /// </summary>
        private void OnInteract()
        {
            
        }
        
        // 상호작용 전용 이벤트
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (TryGetComponent(out IInteractable focused))
            {
                focused.OnFocusEnter(this);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (TryGetComponent(out IInteractable focused))
            {
                focused.OnFocusExit(this);
            }
        }

        #endregion



        
    }
}

