using Character;
using Core.Scripts.Foundation.Define;
using TMPro;
using UnityEngine;

namespace GamePlay.Features.Battle.Scripts.UI.CharacterInfoPopup
{
    public class CharacterInfoPopupStat : MonoBehaviour
    {
        [SerializeField] private TMP_Text statText;
        [SerializeField] private SystemEnum.eStats statType;
        
        public void SetStatText(CharacterModel model)
        {
            statText.text = model.Stat.GetStat(statType).ToString();
        }
    }
}
