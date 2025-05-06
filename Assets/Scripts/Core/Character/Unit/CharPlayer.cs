using AngelBeat.Core.SingletonObjects.Managers;

namespace  AngleBeat
{
    
}
namespace AngelBeat
{
    public class CharPlayer : CharBase
    {
        protected override SystemEnum.eCharType CharType => SystemEnum.eCharType.Player;
        protected override void CharInit()
        {
            base.CharInit();
            BattleCharManager.Instance.SetChar<CharPlayer>(this);
        }
    }
}
