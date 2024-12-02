using System.Collections.Generic;
using UnityEngine;

/// <summary> 대각 이동 제한 </summary>
public enum DiagonalConstraint
{
    None,
    Always,
    DontCrossCorner
}

/// <summary> 타겟 노드 자유도 제한 </summary>
public enum MovableConstraint
{
    None,
    OnlyWalkable,
    AllowWallClick
}

/// <summary> AStar 알고리즘 실행 시 필요한 정보. 외부에서 만들어서 전달할 것. </summary>
public class AStarParameter
{
    /// <summary>알고리즘이 작동할 Grid </summary>
    public AStarGrid        BaseGrid;
    /// <summary>시작 노드</summary>
    public Node             StartNode;
    /// <summary>목표 노드</summary>
    public Node             TargetNode;
    /// <summary>대각 이동시 제약</summary>
    public DiagonalConstraint   Constraint;
    public AStarParameter(AStarGrid pGrid, Node pStart, Node pTarget, DiagonalConstraint constraint)
    {
        BaseGrid = pGrid;
        StartNode = pStart;
        TargetNode = pTarget;
        Constraint = constraint;
    }
}

public static class AStarPathFinder
{
    const int STRAIGHT_COST = 10;
    const int DIAGONAL_COST = 14;

    /// <summary> Astar 알고리즘 실행 메서드. 외부에서 파라미터 생성 후, 이 메서드로 실행 </summary>
    /// <param name="param">
    /// 실행 배경 그리드, 시작/최종 노드, 대각 이동 제한을 포함한다.
    /// 알고리즘을 사용할 객체에서 선언 후 적용할 것.
    /// </param>
    public static List<GridPoint> AStarPath(AStarParameter param)
    {
        #region INIT
        var openList =                  new Heap<Node>();
        var closedList =                new List<Node>();
        var grid =                      param.BaseGrid;
        var startNode =                 param.StartNode;
        var targetNode =                param.TargetNode;
        DiagonalConstraint constraint =     param.Constraint;
        #endregion

        openList.Add(startNode);

        while (openList.Count > 0)
        {
            var current = openList.Pop();
            if(current == targetNode)
            {
                return ReconstructPath(current);
            }
            closedList.Add(current);

            var neighbors = grid.GetNeighbors(current, constraint);
            foreach (var neighbor in neighbors)
            {
                if (closedList.Contains(neighbor)) continue;

                int tentative_G = current.G + 
                    (((Mathf.Abs(current.x - neighbor.x) == 1)&&(Mathf.Abs(current.y - neighbor.y) == 1)) ? 
                    DIAGONAL_COST : STRAIGHT_COST);
                if (!openList.Contains(neighbor) || tentative_G < neighbor.G)
                {
                    neighbor.ParentNode = current;
                    neighbor.G = tentative_G;
                    neighbor.H = OctileHeuristic(current, targetNode);
                    if(!openList.Contains(neighbor)) 
                        openList.Add(neighbor);
                }
            }
        }

        return null;
    }

    public static List<GridPoint> ReconstructPath(Node targetNode)
    {
        var FinalPoints = new List<GridPoint> { new GridPoint(targetNode.x, targetNode.y) };
        while (targetNode.ParentNode is not null)
        {
            targetNode = targetNode.ParentNode;
            FinalPoints.Add(new GridPoint(targetNode.x, targetNode.y));
        }
        FinalPoints.Reverse();
        return FinalPoints;
    }

    public static int OctileHeuristic(Node current, Node targetNode)
    {
        int x = Mathf.Abs(current.x - targetNode.x);
        int y = Mathf.Abs(current.y - targetNode.y);

        int diagonal = DIAGONAL_COST * Mathf.Max(x, y);
        int straight = STRAIGHT_COST * Mathf.Abs(x - y);

        return diagonal + straight;
    }

    //public static Node FindNearestWalkable(Node target)
    //{

    //}
}

/// <summary>
/// 추상 좌표에서의 결과 변환. 
/// 좌표계를 어떻게 설정하냐에 따라, List<GridPoint>를 매개로 하여 변환해주는 메서드를 여기에 추가해준다. 
/// </summary>
public static class PointConverter
{
    public static List<Vector2Int> ToVector2Int(List<GridPoint> points)
    {
        if(points is null) return null;
        var result = new List<Vector2Int>();
        foreach(var point in points)
        {
            result.Add(new Vector2Int(point.x, point.y));
        }
        return result;
    }

    public static List<Vector2> ToIsoVec2(List<GridPoint> points, GridPoint pivot)
    {
        return null;
    }
}

