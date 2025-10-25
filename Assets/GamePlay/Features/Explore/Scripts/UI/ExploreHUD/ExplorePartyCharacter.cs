using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class ExplorePartyCharacterUI : MonoBehaviour
{
    [SerializeField] private Image characterPortrait;
    [SerializeField] private TMP_Text characterName;
    [SerializeField] private Image hpFill;

    public void InitExplorePartyCharacter(Sprite img, string charName, long curHp, long maxHp)
    {
        characterPortrait.sprite = img;
        characterName.text = charName;
        hpFill.fillAmount = curHp / (float)maxHp;
    }
}
