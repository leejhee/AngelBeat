using AngelBeat.Core;
namespace AngelBeat.UI
{
    public class UI_Scene : UI_Base
    {
        public override void Init()
        {
            UIManager.Instance.SetCanvas(gameObject, false);
        }
    }
}

