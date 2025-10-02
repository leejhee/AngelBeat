using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.Features.Battle.Scripts.UI.UIObjects
{
    public class TurnPortrait : MonoBehaviour
    {
        [SerializeField] private Image character;
        [SerializeField] private Image background;
        [SerializeField] private Image currentTurnIndicator;
        [SerializeField] private long charUID;
        public void OnCharacterDie()
        {
            // 캐릭터 사망시 배경화면 어둡게 해주기
            Debug.Log("캐릭터 사망");

        }

        public void SetCurrentTurn(bool isTurn)
        {
            currentTurnIndicator.gameObject.SetActive(isTurn);
        }

        public void SetPortraitImage(Sprite sprite, long UID)
        {
            character.sprite = sprite;
            charUID = UID;
        }
    }
}
