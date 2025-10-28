using AngelBeat.UI;
using GamePlay.Features.Battle.Scripts.UI.CharacterInfoPopup;
using GamePlay.Features.Battle.Scripts.UI.IngameUI;
using UIs.Runtime;
using UnityEngine;

namespace GamePlay.Features.Battle.Scripts.UI
{
    [CreateAssetMenu(fileName = "BattlePresenterFactory", menuName = "ScriptableObject/UIPresenter/BattlePresenterFactory")]
    public class BattlePresenterFactory : PresenterFactory
    {
        public override IPresenter Create(ViewID id, IView view, object param = null)
        {
            switch (id)
            {
                case ViewID.BattleSceneView:
                    return new BattleHUDPresenter(view);
                case ViewID.CharacterInfoPopUpView:
                    return new CharacterInfoPresenter(view);
                case ViewID.GameWinView:
                    return new BattleWinPresenter(view);
                case ViewID.GameOverView:
                    return new GameOverPresenter(view);
                case ViewID.CharacterView:
                    return new HoveringUIPresenter(view);
                default:
                    return new NullPresenter(view);
            }
        }
    }
}