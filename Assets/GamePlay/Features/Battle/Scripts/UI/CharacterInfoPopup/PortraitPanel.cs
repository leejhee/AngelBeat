using Character;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.Features.Battle.Scripts.UI.CharacterInfoPopup
{
    public class PortraitPanel : MonoBehaviour
    {
        [SerializeField] private Image characterPortrait;
        [SerializeField] private Image characterName;
        [SerializeField] private Image characterClass;
        public void SetPortraitPanel(CharacterModel model)
        {
            Debug.Log("팝업창 켰을때 캐릭터 초상화 세팅");
        }
    }
}
