using Core.Scripts.Foundation.Define;
using GamePlay.Character.Components;
using GamePlay.Common.Scripts.Entities.Character;
using GamePlay.Features.Battle.Scripts;
using GamePlay.Features.Battle.Scripts.Unit;

//using Modules.BT;

namespace GamePlay.Features.Scripts.Battle.Unit
{
    public class CharMonster : CharBase
    {
        private CharAI _charAI;
        protected override SystemEnum.eCharType CharType => SystemEnum.eCharType.Enemy;
        
        
        protected override void CharInit()
        {
            base.CharInit();
            BattleCharManager.Instance.SetChar(this);
            
            //CharInfo = new CharacterModel(Index);
            //BTContext context = new(this);
            //_charAI = new CharAI(context);
        }
        
        //private Coroutine _aiRoutine;
        public void StartAI()
        {
            //_aiRoutine =  StartCoroutine(_charAI.AIRoutine());
        }

        public void StopAI()
        {
            //if(_aiRoutine != null)
            //    StopCoroutine(_aiRoutine);
        }
    
    }
}
