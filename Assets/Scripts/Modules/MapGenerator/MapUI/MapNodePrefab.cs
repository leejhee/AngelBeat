using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MapNodePrefab : UI_Base
{
    // Image or sth. it'll be set in SetNodeInfo
    public enum Images
    {
        NodeImage
    }
    
    public enum Buttons
    {
        NodeButton,
    }

    //[TODO] : 나머지 필요한거 봐서 알아서 짜라.
    public override void Init()
    {
        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));
    }

    /// <summary>
    /// Set Node's data
    /// </summary>
    public void SetNodeInfo()
    {

    }

    /// <summary>
    /// 일단 클릭인데... 이거 어떻게 되게 할 지. 
    /// </summary>
    /// <param name="evt"></param>
    public void OnClickPrefab(PointerEventData evt)
    {

    }
}