using AngelBeat;
using Core.Scripts.Foundation.Define;
using GamePlay.Features.Battle.Scripts.Unit;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace GamePlay.Features.Battle.Scripts.UI
{
    public class CharacterHUD : MonoBehaviour
    {
        #region Objects
        [SerializeField] private Button characterPortraitButton;
        [SerializeField] private Image characterPortraitImage;
        [SerializeField] private TextMeshProUGUI characterName;
        
        [SerializeField] private SkillButtonPanel skillPanel;
        
        public Button CharacterPortraitButton => characterPortraitButton;
        public SkillButtonPanel SkillPanel => skillPanel;
        
        #region Bar
        [SerializeField] private Image hpBarFill;
        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private Image actionBarFill;
        [SerializeField] private TextMeshProUGUI actionText;
        #endregion

        #region ExtraAction
        [SerializeField] private Button jumpButton;
        [SerializeField] private Button pushButton;
        [SerializeField] private Button invenButton;
        
        public Button  JumpButton => jumpButton;
        public Button  PushButton => pushButton;
        public Button  InvenButton => invenButton;
        
        #endregion
        #endregion
        
        public void ShowCharacterHUD(
            long curHp,
            long maxHp,
            long curAp,
            long maxAp,
            Sprite portrait = null)
        {
            // 초상화 변경
            //characterPortraitImage.sprite = curCharacter.
            // 현재 체력
            hpText.text = 
                $"{curAp}/{maxHp}";
            // 현재 액션포인트
            actionText.text = 
                $"{curAp}/{maxAp}";
        }

        public void ReduceHpUI(int reducedHp)
        {
            CharBase focus = BattleController.Instance.FocusChar;
            string newHpText = 
                $"{(SystemEnum.eStats.NHP)}" + "/" + 
                $"{focus.CharStat.GetStat(SystemEnum.eStats.NMHP)}";
            hpText.text = newHpText;
            // hpBarFill 변경해주기
        }

        public void ReduceActionUI(int reducedActionPoint)
        {
            CharBase focus = BattleController.Instance.FocusChar;
            string newActionText =
                $"{(SystemEnum.eStats.NACTION_POINT)}" + "/" + 
                $"{focus.CharStat.GetStat(SystemEnum.eStats.NMACTION_POINT)}";
            actionText.text = newActionText;
            // actionBarFill 변경해주기
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }
        public void Hide() => gameObject.SetActive(false);
    }
}
