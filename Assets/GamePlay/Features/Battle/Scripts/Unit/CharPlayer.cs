using Core.Scripts.Foundation.Define;
using Cysharp.Threading.Tasks;
using GamePlay.Common.Scripts.Entities.Character;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GamePlay.Features.Battle.Scripts.Unit
{
    public class CharPlayer : CharBase, IPointerClickHandler
    {
        
        protected override SystemEnum.eCharType CharType => SystemEnum.eCharType.Player;
        public override async UniTask CharInit(CharacterModel charModel)
        {
            await base.CharInit(charModel);
            BattleCharManager.Instance.SetChar(this);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            if (eventData.clickCount < 2) return;
            
            var bc = BattleController.Instance;
            if (bc.IsModal) return;
            if (bc.FocusChar != this) return;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

        }
        
    }
}
