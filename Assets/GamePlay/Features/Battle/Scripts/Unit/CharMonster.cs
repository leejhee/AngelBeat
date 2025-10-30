using Core.Scripts.Foundation.Define;
using Cysharp.Threading.Tasks;
using GamePlay.Character.Components;
using GamePlay.Common.Scripts.Entities.Character;
using GamePlay.Features.Battle.Scripts.BattleTurn;
using UnityEngine;


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
            _charAI = new CharAI(this);
        }
        
        public async UniTask ExecuteAITurn(Turn turn)
        {
            if (_charAI == null)
            {
                Debug.LogWarning($"{name} AI 미초기화");
                return;
            }
            await _charAI.ExecuteTurn(turn);
        }
    }
}
