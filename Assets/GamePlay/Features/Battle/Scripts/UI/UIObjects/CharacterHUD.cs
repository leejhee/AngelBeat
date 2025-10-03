using AngelBeat;
using Cysharp.Threading.Tasks;
using GamePlay.Common.Scripts.Entities.Skills;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.Features.Battle.Scripts.UI.UIObjects
{
    public class CharacterHUD : MonoBehaviour
    {
        #region Objects
        //[SerializeField] private Button characterPortraitButton;
        [SerializeField] private Image characterPortraitImage;
        [SerializeField] private TMP_Text characterName;
        [SerializeField] private SkillButtonPanel skillPanel;
        [SerializeField] private CharacterPortrait characterPortrait;

        public CharacterPortrait CharacterPortrait => characterPortrait;
        //public Button CharacterPortraitButton => characterPortraitButton;
        public SkillButtonPanel SkillPanel => skillPanel;
        
        #region Bar
        [SerializeField] private Image hpBarFill;
        [SerializeField] private TMP_Text hpText;
        [SerializeField] private Slider hpSlider;
        [SerializeField] private Image actionBarFill;
        [SerializeField] private TMP_Text actionText;
        [SerializeField] private Slider actionSlider;
        #endregion

        #region ExtraAction
        [SerializeField] private ExtraActionButton jumpButton;
        [SerializeField] private ExtraActionButton pushButton;
        [SerializeField] private ExtraActionButton invenButton;
        
        public ExtraActionButton  JumpButton => jumpButton;
        public ExtraActionButton  PushButton => pushButton;
        public ExtraActionButton  InvenButton => invenButton;
        
        #endregion
        #endregion
        
        public void ShowCharacterHUD(
            string charName,
            long curHp,
            long maxHp,
            long curAp,
            long maxAp,
            Sprite portrait = null)
        {
            // 초상화 변경
            //characterPortraitImage.sprite = curCharacter.
            // 이름 변경
            characterName.text = charName;
            
            // 현재 체력
            hpText.text = 
                $"{curAp}/{maxHp}";
            //체력바 슬라이더 설정
            hpSlider.maxValue = maxHp;
            hpSlider.value = curAp;
            
            // 현재 액션포인트
            actionText.text = 
                $"{curAp}/{maxAp}";
            // 액션바 슬라이더 설정
            actionSlider.maxValue = maxAp;
            actionSlider.value = curAp;
        }

        public void SetSkillButtons(IReadOnlyList<SkillModel> skillList)
        {
            int skillCount = skillList.Count;
            for (int i = 0; i < skillPanel.SkillButtons.Count; i++)
            {
                bool isSkill = i < skillCount;
                skillPanel.SkillButtons[i].gameObject.SetActive(isSkill);
                if (isSkill)
                {
                    int idx = i;
                    SkillButton button = skillPanel.SkillButtons[idx];
                    button.SetButton(skillList[idx]);
                    
                    // 여기 부분 프리젠트로 옮기기
                    // button.GetComponent<Button>().onClick.RemoveAllListeners();
                    // button.GetComponent<Button>().onClick.AddListener(() =>
                    // {
                    //     BattleController.Instance.ShowSkillPreview(skillList[idx]);
                    //     Debug.Log($"Skill {skillList[idx].SkillName} Selected");
                    // });
                    // 여기까지
                }
            }
            
        }

        private async UniTask HpOrApBarChanged(long delta, Slider slider, TMP_Text text)
        {
            const float TIME_TO_END = 0.5f;
            float counter = 0f;
            float startValue = slider.value;
            float targetValue = startValue + delta;
            while (counter < TIME_TO_END)
            {
                counter += Time.deltaTime;
                float t = counter/TIME_TO_END;
                float curValue = Mathf.Lerp(startValue, targetValue, t);
                slider.value = curValue;
                text.text = $"{(int)curValue}/{(int)slider.maxValue}";
                await  UniTask.Yield();
            }
            slider.value = targetValue;
            text.text = $"{(int)targetValue}/{(int)slider.maxValue}";
        }
        public async void ChangeHpUI(long delta)
        {
            await HpOrApBarChanged(delta, hpSlider, hpText);
        }
        public async void ChangeApUI(long delta)
        {
            await HpOrApBarChanged(delta, actionSlider, actionText);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }
        public void Hide() => gameObject.SetActive(false);
    }
    
}
