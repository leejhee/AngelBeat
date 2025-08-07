using Character;
using GamePlay.Character;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay.Explore
{
    public class ExploreController : MonoBehaviour
    {
        [SerializeField] private float speed;
        [SerializeField] private Party playerParty;
        public Party PlayerParty => playerParty;
        
        private void Start()
        {
            var testXiaoModel = new CharacterModel(88888888);
            playerParty = new Party(new List<CharacterModel> { testXiaoModel });
            Debug.Log($"{playerParty.SearchCharacter("샤오").Name}");
        }

        private void OnEnable()
        {
            InputManager.Instance.KeyAction -= MoveInput;
            InputManager.Instance.KeyAction += MoveInput;
        }

        private void OnDisable()
        {
            InputManager.Instance.KeyAction -= MoveInput;
        }

        private void MoveInput()
        {
            MoveByKeyboardInput();
            MoveByMouseInput();
        }
        
        private void MoveByKeyboardInput()
        {
            float moveX = 0f;
            float moveY = 0f;
    
            if (Input.GetKey(KeyCode.W)) moveY += 0.5f;
            if (Input.GetKey(KeyCode.S)) moveY -= 0.5f;
            if (Input.GetKey(KeyCode.A)) moveX -= 1f;
            if (Input.GetKey(KeyCode.D)) moveX += 1f;
    
            Vector3 moveDir = new Vector3(moveX, moveY, 0f).normalized;
            transform.position += moveDir * (speed * Time.deltaTime);
        }

        private void MoveByMouseInput()
        {
            //매 프레임 마우스의 입력을 감지
            if (Input.GetMouseButton(0))
            {
                // 도착지 감지해서 A* 돌려라잇
            }
        }
        
    }
}

