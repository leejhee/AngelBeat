using GamePlay.Common.Scripts.Entities.Skills;
using GamePlay.Skill;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SkillDescription : MonoBehaviour
{
    [SerializeField] private TMP_Text skillName;
    [SerializeField] private TMP_Text skillDescription;
    [SerializeField] private Sprite skillDescriptionSprite;

    public void SetSkillDescription(SkillModel model)
    {
        //skillName.text = model.SkillName;
        // 스킬 설명 추가하기
        
    }
}
