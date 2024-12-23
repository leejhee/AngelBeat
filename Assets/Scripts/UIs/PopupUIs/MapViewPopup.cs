using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 일단 지금은 팝업인데, 씬 자체로 사용할지는 합의 예정
public class MapViewPopup : UI_Popup
{
    public enum MapOrientation
    {
        Left2Right,
        Right2Left,
        Top2Bottom,
        Bottom2Top
    }

    [Header("컴파일타임에 편집하세요 - 맵 세부 변경 사항")]
    public MapOrientation orientation;
    public float MapPrefabInterval = 150;
    public float PopupContentWidthOffset;
    public float PopupContentHeightOffset;
     
    enum Buttons
    {
        CloseBtn,
    }

    enum Transforms 
    {
        Map
    }

    public override void Init()
    {
        base.Init();
        Bind<Transform>(typeof(Transforms));
        Bind<Button>(typeof(Buttons)); 
        BindEvent(Get<Button>((int)Buttons.CloseBtn).gameObject, OnClickCloseBtn);

        InstantiatePoints();
        DrawPaths();
    }

    public void OnClickCloseBtn(PointerEventData evt)
    {
        UIManager.Instance.ClosePopupUI();
    }

    public void InstantiatePoints()
    {
        var ScrollContent = Get<Transform>((int)Transforms.Map);
        var Map = StageManager.Instance.StageMap;
        foreach(MapFloor floor in Map.MapNodes)
        {
            foreach(MapNode node in floor.FloorMembers)
            {
                var position = node.GridPoint;
                var nodePrefab = ResourceManager.Instance.Instantiate<MapNodePrefab>
                    ("UI/MapNodePrefab", ScrollContent);
                nodePrefab.transform.localPosition = 
                    new Vector3(position.x * MapPrefabInterval, position.y * MapPrefabInterval, 0);
                nodePrefab.SetNodeInfo(node.NodeData);
            }
        }
    }

    public void DrawPaths()
    {

    }


}
