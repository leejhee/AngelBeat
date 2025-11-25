using GamePlay.Common.Scripts.Entities.Character;
using GamePlay.Contracts.Interaction;
using System;
using System.Threading;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GamePlay.Features.Explore.Scripts
{
    /// <summary>
    /// '탐사' 씬에서 유저가 조종할 컨트롤러.
    /// </summary>
    public class ExploreController : MonoBehaviour, IInteractor
    {
        [SerializeField] private float speed = 3f;
        [SerializeField] private Party playerParty;
        public Party PlayerParty => playerParty;
        
        public Transform Transform { get; }
        public CancellationToken LifeTimeToken { get; }
        public Transform CameraTransform;

        private Rigidbody2D _rb;
        private Vector2 _moveInput;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void Start()
        {
            CinemachineCamera cam = CameraTransform.gameObject.GetComponent<CinemachineCamera>();
            LensSettings lens = cam.Lens;
            lens.OrthographicSize = 1f;
        }
        
        private void Update()
        {
            Vector2 move = Vector2.zero;
            var keyboard = Keyboard.current;

            if (keyboard != null)
            {
                if (keyboard.wKey.isPressed) move.y += 1f;
                if (keyboard.sKey.isPressed) move.y -= 1f;
                if (keyboard.aKey.isPressed) move.x -= 1f;
                if (keyboard.dKey.isPressed) move.x += 1f;
            }

            if (move.sqrMagnitude > 1f)
                move = move.normalized;

            _moveInput = move;
        }

        private void FixedUpdate()
        {
            var targetPos = _rb.position + _moveInput * speed * Time.fixedDeltaTime;
            _rb.MovePosition(targetPos);
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
            if (other.TryGetComponent(out IInteractable focused))
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

