using UIs.Runtime;

namespace GamePlay.Features.Village.Scripts.UI
{
    public class VillagePresenterFactory : PresenterFactory
    {
        public override IPresenter Create(ViewID id, IView view, object param = null)
        {
            switch (id)
            {
                case ViewID.VillageToExploreInteractionView:
                    return new VillageToExploreInteractionPresenter(view);
                default:
                    return new NullPresenter();
            }
            
        }
    }
}