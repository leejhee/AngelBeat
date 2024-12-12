using UnityEngine;
using UnityEngine.UI;

public class MapNodePrefab : UI_Base
{

    public enum Buttons
    {
        NodeButton,
    }



    //[TODO] : 나머지 필요한거 봐서 알아서 짜라.
    public override void Init()
    {
        Bind<Button>(typeof(Buttons));

    }
}