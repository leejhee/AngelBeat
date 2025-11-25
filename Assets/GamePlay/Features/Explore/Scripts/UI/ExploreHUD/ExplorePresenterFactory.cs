using GamePlay.Features.Battle.Scripts.UI.CharacterInfoPopup;
using UIs.Runtime;
using UnityEngine;

namespace GamePlay.Features.Explore.Scripts.UI
{
    [CreateAssetMenu(fileName = "ExplorePresenterFactory", menuName = "ScriptableObject/UIPresenter/ExplorePresenterFactory")]
    public class ExplorePresenterFactory : PresenterFactory
    {
        public override IPresenter Create(ViewID id, IView view, object param = null)
        {
            switch (id)
            {
                case ViewID.ExploreSceneView:
                    return new ExploreHUDPresenter(view);
                case ViewID.CharacterInfoPopUpView:
                    return new CharacterInfoPresenter(view);
                default:
                    return new NullPresenter(view);
            }
        }
    }
}