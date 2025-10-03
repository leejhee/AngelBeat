using Core.Scripts.Foundation.Define;
using GamePlay.Common.Scripts.Entities.Character;
using UnityEngine;

namespace GamePlay.Features.Battle.Scripts.Unit
{
    public class CharPlayer : CharBase
    {
        
        protected override SystemEnum.eCharType CharType => SystemEnum.eCharType.Player;
        public override void CharInit(CharacterModel charModel)
        {
            base.CharInit(charModel);
            BattleCharManager.Instance.SetChar(this);
        }

        public void CharMove(Vector3 Destination)
        {
            
        }
        
    }
}
