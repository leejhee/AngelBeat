using AngelBeat.Core.Character;
using UnityEngine;

namespace AngelBeat.Core.Explore
{
    public class ExploreController : MonoBehaviour
    {
        #region Mover for Demo
        [SerializeField] private float speed;
        void Update()
        {
            float moveX = 0f;
            float moveY = 0f;
    
            if (Input.GetKey(KeyCode.W)) moveY += 1f;
            if (Input.GetKey(KeyCode.S)) moveY -= 1f;
            if (Input.GetKey(KeyCode.A)) moveX -= 1f;
            if (Input.GetKey(KeyCode.D)) moveX += 1f;
    
            Vector3 moveDir = new Vector3(moveX, moveY, 0f).normalized;
            transform.position += moveDir * (speed * Time.deltaTime);
        }
        #endregion

        [SerializeField] private Party playerParty;
        public Party PlayerParty => playerParty;

    }
}

