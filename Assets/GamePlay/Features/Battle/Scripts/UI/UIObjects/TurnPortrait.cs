using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.Features.Battle.Scripts.UI.UIObjects
{
    public class TurnPortrait : MonoBehaviour
    {
        [SerializeField] private Image character;
        [SerializeField] private Image background;

        public void OnCharacterDie()
        {
            // 캐릭터 사망시 배경화면 어둡게 해주기
        }
    }
}
