using GamePlay.Features.Battle.Scripts.Unit;
using GamePlay.Features.Scripts.Battle.Unit;
using UnityEngine;

namespace AngelBeat
{
    public class DeathFloor : MonoBehaviour
    {
        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.TryGetComponent(out CharBase character))
            {
                character.CharDead();
            }
        }
    }
    
}
