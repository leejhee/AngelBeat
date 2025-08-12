using AngelBeat.Core.Battle;
using AngelBeat.Core.SingletonObjects.Managers;
using Character.Unit;
using Core.Foundation;
using Core.Foundation.Define;
using GamePlay.Skill;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AngelBeat.UI
{
    public class BattleSceneUI : MonoBehaviour
    {
        #region UI
        [SerializeField] private Button moveButton;
        [SerializeField] private Button turnEndButton;
        [SerializeField] private TMP_Text turnOwnerText;
        
        [SerializeField] private GameObject panelPrefab;
        private SkillButtonPanel _skillButtonPanel;
        #endregion
        
        
        
        private void Awake()
        {
            _skillButtonPanel = panelPrefab.GetComponent<SkillButtonPanel>();
            
            #region Model 변경 구독
            EventBus.Instance.SubscribeEvent<OnTurnChanged>(this, OnTurnChange);
            #endregion
            
            #region UI 입력 구독
            turnEndButton.onClick.AddListener(OnTurnEndClick);
            moveButton.onClick.AddListener(OnClickMove);
            #endregion
        }
        
        private void OnDestroy()
        {
            EventBus.Instance.UnsubscribeEvent(this);
        }
        
        // 지금은 그냥 CharBase가 CharacterModel처럼 역할한다고 생각하셈
        /// <summary> 턴이 바뀐다 = 포커스 정보가 바뀐다. </summary>
        /// TODO : 캐릭터 정보, 이동 및 스킬 UI 변경에 모두 연결되어야 함. 
        private void OnTurnChange(OnTurnChanged info)
        {
            CharBase turnOwner = info.TurnOwner;
            turnOwnerText.text = turnOwner.name;
            if (turnOwner.GetCharType() == SystemEnum.eCharType.Player)
            {
                IReadOnlyList<SkillModel> skillList = turnOwner.CharInfo.Skills;
                _skillButtonPanel.SetSkillButtons(turnOwner, skillList); 
            }
            //UI 변경
            
            
            
        }

        private void OnTurnEndClick()
        {
            EventBus.Instance.SendMessage(new OnTurnEndInput());
            Debug.Log("turn end input");
        }
        
        private void OnClickMove()
        {
            EventBus.Instance.SendMessage(new OnMoveInput());
            //이동 관련 미리보기 UI를 띄우게 해야함.
            //이건 Preview UI쪽에서 뜨게 해야함.
            //일단 플랫폼간 이동만 되게 할거니까, Preview UI쪽에서는 해당 클릭이 들어가면, 포커스쪽에다가 오브젝트를 활성화시키는 쪽으로
            //하는게 낫겠다.
            Debug.Log("move input");
        }
        
    }
}



