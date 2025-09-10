using Core.Scripts.Foundation.Define;
using UnityEngine;

namespace GamePlay.Features.Scripts.Battle.Unit
{
    public class CharPlayer : CharBase
    {
        
        protected override SystemEnum.eCharType CharType => SystemEnum.eCharType.Player;
        protected override void CharInit()
        {
            base.CharInit();
            BattleCharManager.Instance.SetChar<CharPlayer>(this);
        }
        
        // 턴이 되면 -> _turnManager
        // 플레이어의 입력을 Update에서 처리할 수 있게 해준다.
        // 
        public void OnPlayerMoveUpdate()
        {
            if (Input.GetKey(KeyCode.A))
            {
                CharMove(Vector3.left);
            }
            else if (Input.GetKey(KeyCode.D))
            {
                CharMove(Vector3.right);
            }
            else if (Input.GetKeyDown(KeyCode.Space) && IsGrounded)
            {
                StartCoroutine(CharJump());
            }
            else if (Input.GetKeyDown(KeyCode.S) && IsGrounded)
            {
                StartCoroutine(CharDownJump());
            }
        }
        
    }
}
