using Core.Scripts.Foundation.Define;
using Cysharp.Threading.Tasks;
using GamePlay.Common.Scripts.Entities.Character;
using UnityEngine;

namespace GamePlay.Features.Battle.Scripts.Unit
{
    public class CharPlayer : CharBase
    {
        
        protected override SystemEnum.eCharType CharType => SystemEnum.eCharType.Player;
        public override async UniTask CharInit(CharacterModel charModel)
        {
            await base.CharInit(charModel);
            BattleCharManager.Instance.SetChar(this);
        }
        
        
    }
}
