using AngelBeat;
using Cysharp.Threading.Tasks;
using GamePlay.Common.Scripts.Entities.Skills;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.Features.Battle.Scripts.UI.UIObjects
{
    public abstract class ToggleButton : MonoBehaviour
    {
        public bool selectable;
        public bool isSelected;
        [SerializeField] protected Image frame;
        [SerializeField] protected Sprite selectedFrame;
        [SerializeField] protected Sprite nonSelectedFrame;
        public Image Frame => frame;
        public Sprite SelectedFrame => selectedFrame;
        public Sprite NonSelectedFrame => nonSelectedFrame;

        private void Start()
        {
            selectable = true;
        }

        public abstract void OnSelect();

        public abstract void OnDeselect();
    }
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
        
        
        [SerializeField] private List<ToggleButton> buttons = new List<ToggleButton>();

        private void Start()
        {
            foreach (var button in buttons)
            {
                button.GetComponent<Button>().onClick.AddListener(() => Toggling(button));
            }
        }

        public void Toggling(ToggleButton selectedButton)
        {
            // 이미 선택된 버튼을 클릭시
            if (selectedButton.isSelected)
            {
                selectedButton.isSelected = false;
                selectedButton.Frame.sprite = selectedButton.NonSelectedFrame;
                selectedButton.OnDeselect();
            }
            else
            {
                if (!selectedButton.selectable) return; // 선택할 수 없는 경우에는 토글이 되면 안된다.
                foreach (var button in buttons)
                {
                    if (button.isSelected)
                    {
                        button.isSelected = false;
                        button.Frame.sprite = button.NonSelectedFrame;
                        button.OnDeselect();
                    }

                }
                selectedButton.isSelected = true;
                selectedButton.Frame.sprite = selectedButton.SelectedFrame;
                selectedButton.OnSelect();
            }
        }
        
        // 스킬이나 행동이 사용되었을 경우
        public void DisableAllToggleButton()
        {
            foreach (var button in buttons)
            {
                if (button.isSelected)
                {
                    button.isSelected = false;
                    button.Frame.sprite = button.NonSelectedFrame;
                    //return;
                }

                button.selectable = false;
            }
        }
        
        public void ShowCharacterHUD(
            string charName,
            long curHp,
            long maxHp,
            long curAp,
            long maxAp,
            Sprite portrait = null)
        {
            // TODO : 튜토
            //characterPortraitImage.sprite = curCharacter.
            // 이름 변경
            
            characterName.text = charName;
            
            // 현재 체력
            hpText.text = 
                $"{curHp}/{maxHp}";
            //체력바 슬라이더 설정
            hpSlider.maxValue = maxHp;
            hpSlider.value = curHp;
            
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
                SkillButton button = skillPanel.SkillButtons[i];
                skillPanel.SkillButtons[i].gameObject.SetActive(isSkill);
                if (!isSkill) continue;
                
                button.BindSlot(i);
                button.SetButton(skillList[i]);
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
