using Character;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.Features.Battle.Scripts.UI.CharacterInfoPopup
{
    public class PassivePanel : MonoBehaviour
    {
        [SerializeField] private Image normalPassive;
        [SerializeField] private Image additivePassive;

        public void SetPassivePanel(CharacterModel model)
        {
            Debug.Log("패시브 스킬 이미지 세팅");
        }
    }
}
