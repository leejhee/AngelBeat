using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;


#pragma warning disable IDE0051
public static class MapGenerator
{
    //  [TODO]
    //  1. 입력 시드 통해서 생성한 난수를 통해 맵을 생성하도록 한다.
    //  2. 보스는 하나일 지. 최종맵에서 2개라던가...할건지.
    //  3. 노드 포인팅 시, 시작과 끝 노드는 완전히 다른 블록으로 빼서 초기화하는게 좋을듯.    
    /// <summary> 0층과 끝층을 제외, 횡방향이 아닌, 종방향 형태로 맵 생성 </summary>
    private static List<MapFloor> GetNodePoints(int maxDepth, int trialNum, int width)
    {
        int id = 1;
        var nodes = new List<MapFloor>();
        /*
        //List<int> GiveCandidate(GridPoint point)
        //{
        //    #region field init
        //    int left =      point.x - 1;
        //    int middle =    point.x;
        //    int right =     point.x + 1;
        //    int y =         point.y;
        //    List<int> candidates = new List<int> { left, middle, right };
        //    #endregion
        //    #region clamp candidate x
        //    if (left < 0)       candidates.Remove(left);
        //    if (right >= width) candidates.Remove(right);
        //    if (candidates.Count <= 1) return candidates; //여기에 디버그가 걸리면 안된다(비정상적 맵 파라미터.)
        //    #endregion
        //    #region investigate cross path
        //    MapNode criterion = nodes[point.y + 1].GetNode(middle);
        //    if (criterion != null)
        //    {
        //        bool crossLeft = candidates.Contains(left) &&
        //                        nodes[y].GetNode(left).Children.Contains(criterion);
        //        bool crossRight = candidates.Contains(right) &&
        //                        nodes[y].GetNode(right).Children.Contains(criterion);
        //        if (crossLeft) candidates.Remove(left);
        //        if (crossRight) candidates.Remove(right);
        //    }

        //    return candidates;
        //    #endregion
        //}
        */

        #region Pointing Nodes
        for (int floorNum = 0; floorNum <= maxDepth; floorNum++)
            nodes.Add(new MapFloor(floorNum)); // 맵의 층(시작층 ~ 최종층) 생성

        for (int i = 0; i < trialNum; i++)
        {
            var lineStartNode = nodes[1].AddNode(new MapNode(id, Random.Range(0, width), 1));
            id++;
            var current = lineStartNode;
            for (int floorIdx = 1; floorIdx < maxDepth - 2; floorIdx++)
            {
                var nextFloor = nodes[floorIdx + 1];
                List<int> candidates = GiveCandidates(nodes, current.GridPoint, width);
                int electedX = candidates[Random.Range(0, candidates.Count)];
                
                MapNode nextNode = null;
                nextNode = nextFloor.AddNode(new MapNode(id, electedX, floorIdx + 1));

                // Path 형성. 여기서 Path 객체도 형성할지 고민 중
                current.SetChildren(nextNode, true);
                nextNode.SetParents(current, true);
                current = nextNode;
                id++;
            }
        }
        #endregion

        foreach (var floor in nodes)
        {
            floor.FloorMembers.Sort();
        }

        return nodes;
    }

    private static List<int> GiveCandidates(List<MapFloor> nodes, GridPoint Point, int rightClamp)
    {
        #region field init
        int left = Point.x - 1;
        int middle = Point.x;
        int right = Point.x + 1;
        int y = Point.y;
        List<int> candidates = new List<int> { left, middle, right };
        #endregion
        #region clamp candidate x
        if (left < 0) candidates.Remove(left);
        if (right >= rightClamp) candidates.Remove(right);
        if (candidates.Count <= 1) return candidates; //여기에 디버그가 걸리면 안된다(비정상적 맵 파라미터.)
        #endregion
        #region investigate cross path
        MapNode criterion = nodes[Point.y + 1].GetNode(middle);
        if (criterion != null)
        {
            bool crossLeft = candidates.Contains(left) &&
                            nodes[y].GetNode(left).Children.Contains(criterion);
            bool crossRight = candidates.Contains(right) &&
                            nodes[y].GetNode(right).Children.Contains(criterion);
            if (crossLeft) candidates.Remove(left);
            if (crossRight) candidates.Remove(right);
        }

        return candidates;
        #endregion
    }


    private static List<MapPath> GiveMapPaths(List<MapFloor> AllNodes)
    {
        var paths = new List<MapPath>();

        foreach (var floor in AllNodes)
        {
            foreach(var node in floor.FloorMembers)
            {
                foreach(var child in node.Children)
                {
                    var path = new MapPath(node.NodeID, child.NodeID);
                    paths.Add(path);
                }
            }
        }

        return paths;
    }

    
    /// <summary>
    /// 최종적으로 맵을 생성한다. 이거로 쓸 것.
    /// </summary>
    public static Map CreateMap(MapParameter param)
    {        
        var mapPoints = GetNodePoints(param.maxDepth, param.trialNum, param.width);
        var mapPaths = GiveMapPaths(mapPoints);

        var map = new Map(mapPoints, mapPaths);
        return map;
    }
}
#pragma warning restore IDE0051

