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
                default:
                    return new NullPresenter();
            }
        }
    }
}