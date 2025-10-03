using Core.Scripts.Foundation.Define;
using Cysharp.Threading.Tasks;
using GamePlay.Character.Components;
using GamePlay.Common.Scripts.Entities.Character;

//using Modules.BT;

namespace GamePlay.Features.Battle.Scripts.Unit
{
    public class CharMonster : CharBase
    {
        private CharAI _charAI;
        protected override SystemEnum.eCharType CharType => SystemEnum.eCharType.Enemy;
        
        public override async UniTask CharInit(CharacterModel charModel)
        {
            await base.CharInit(charModel);
            BattleCharManager.Instance.SetChar(this);
            
        }
    
    }
}
