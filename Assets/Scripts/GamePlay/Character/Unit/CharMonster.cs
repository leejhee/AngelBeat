using AngelBeat;
using AngelBeat.Core.SingletonObjects.Managers;
using Core.Foundation.Define;
using Modules.BT;

namespace Character.Unit
{
    public class CharMonster : CharBase
    {
        private CharAI _charAI;
        protected override SystemEnum.eCharType CharType => SystemEnum.eCharType.Enemy;
        
        
        protected override void CharInit()
        {
            base.CharInit();
            BattleCharManager.Instance.SetChar(this);
            
            CharInfo = new CharacterModel(Index);
            BTContext context = new(this);
            _charAI = new CharAI(context);
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
