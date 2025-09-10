using Character;
using Core.Scripts.Managers;
using GamePlay.Contracts.Interaction;
using GamePlay.Entities.Scripts.Character;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace GamePlay.Features.Scripts.Explore
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
            var testXiaoModel = new CharacterModel(88888888);
            playerParty = new Party(new List<CharacterModel> { testXiaoModel });
            Debug.Log($"{playerParty.SearchCharacter("샤오").Name}");
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
        private void MoveByKeyboardInput()
        {
            //float moveX = 0f;
            //float moveY = 0f;
    //
            //if (Input.GetKey(KeyCode.W)) moveY += 0.5f;
            //if (Input.GetKey(KeyCode.S)) moveY -= 0.5f;
            //if (Input.GetKey(KeyCode.A)) moveX -= 1f;
            //if (Input.GetKey(KeyCode.D)) moveX += 1f;
    //
            //Vector3 moveDir = new Vector3(moveX, moveY, 0f).normalized;
            //transform.position += moveDir * (speed * Time.deltaTime);
        }

        private void MoveByMouseInput()
        {
            ////매 프레임 마우스의 입력을 감지
            //if (Input.GetMouseButton(0))
            //{
            //    // 도착지 감지해서 A* 돌려라잇
            //}
        }

        
    }
}

