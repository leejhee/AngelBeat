using Core.Scripts.Foundation.Define;
using GamePlay.Character.Components;
using GamePlay.Common.Scripts.Entities.Character;

//using Modules.BT;

namespace GamePlay.Features.Battle.Scripts.Unit
{
    public class CharMonster : CharBase
    {
        private CharAI _charAI;
        protected override SystemEnum.eCharType CharType => SystemEnum.eCharType.Enemy;
        
        public override void CharInit(CharacterModel charModel)
        {
            base.CharInit(charModel);
            BattleCharManager.Instance.SetChar(this);
            
        }
    
    }
}
