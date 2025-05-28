using AngelBeat.Core.Character;
using AngelBeat.Core.SingletonObjects.Managers;
using UnityEngine;

namespace AngelBeat
{
    public class CharMonster : CharBase
    {
        private CharAI _charAI;
        private Coroutine _aiRoutine;
        protected override SystemEnum.eCharType CharType => SystemEnum.eCharType.Enemy;
        protected override void CharInit()
        {
            base.CharInit();
            BattleCharManager.Instance.SetChar(this);
            _charAI = new(this);

            CharInfo = new CharacterModel(Index);
        }

        public void StartAI()
        {
            _aiRoutine =  StartCoroutine(_charAI.AIRoutine());
        }

        public void StopAI()
        {
            if(_aiRoutine != null)
                StopCoroutine(_aiRoutine);
        }
    
    }
}
