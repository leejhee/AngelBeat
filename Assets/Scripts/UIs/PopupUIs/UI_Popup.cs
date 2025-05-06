using AngelBeat.Core;

namespace AngelBeat.UI
{
    public abstract class UI_Popup : UI_Base
    {
        public override void Init()
        {
            base.Init();
            UIManager.Instance.SetCanvas(gameObject, true);
        }

        public virtual void ReOpenPopupUI() { }

        public virtual void SetParameter(UIParameter param) { }

    }
 
}
