//using System;
//using System.Collections.Generic;
//using UnityEngine;

//public class JPSPathFinder : MonoBehaviour
//{
//    #region CONST&READONLY
//    public enum Direction
//    {
//        None,
//        Up,
//        Down,
//        Left,
//        Right,
//        UpLeft,
//        UpRight,
//        DownLeft,
//        DownRight,

//        MaxValue
//    }

//    public readonly Vector2Int[] DIRECTIONS =
//    {
//        new(0, 0),   // None
//        new(0, 1),   // 상
//        new(0, -1),  // 하
//        new(-1, 0),  // 좌
//        new(1, 0),   // 우
//        new(-1, 1),  // 좌상
//        new(1, 1),   // 우상
//        new(-1, -1), // 좌하
//        new(1, -1)   // 우하
//    };

//    const int               STRAIGHT_COST = 10;
//    const int               DIAGONAL_COST = 14;   
//    #endregion

//    Node CurrentNode;               // 현재 검사할 노드
//    Node StartNode, TargetNode;     // 시작, 목표 지점의 노드

//    Heap<Node> OpenList;            // 검사 대상 노드가 있는 우선순위 큐
//    List<Node> ClosedList;          // 이미 검사를 마친 노드를 담는 리스트
//    public List<Node> FinalNodeList;       // 최종 경로에 포함되는 노드들을 담은 리스트

//    /// <summary>
//    /// 길찾기 실행 시 접근. 이 함수로만 길찾기 알고리즘 실행하도록 한다.
//    /// </summary>
//    /// <param name="startPos">시작 지점. (플레이어 기준)캐릭터가 있는 지점으로 입력된다.</param>
//    /// <param name="targetPos">목표 지점. (플레이어 기준)마우스 우클릭된 지점으로 입력된다.</param>
//    public void StartPathFinding(Vector2Int startPos, Vector2Int targetPos)
//    {
//        StartNode = new Node(startPos.x, startPos.y, new(0, 0));
//        TargetNode = new Node(targetPos.x, targetPos.y, new(0, 0));
//        FinalNodeList = new List<Node>();
//        ClosedList = new List<Node>();
//        OpenList = new Heap<Node>();
//        OpenList.Add(StartNode);
//        if (JPS() == false)
//        {
//            Debug.Log("JPS Pathfinding Failed.");
//            return;
//        }

//    }
    
//    bool JPS()
//    {
//        while(OpenList.Count > 0)
//        {
//            // 검사 대상 노드 추출
//            CurrentNode = OpenList.Pop();

//            // 목표 노드 도달 시 FinalNodeList 
//            if(CurrentNode == TargetNode)
//            {
//                ReconstructPath();
//                return true;
//            }

//            ClosedList.Add(CurrentNode);

//            List<Node> Neighbors = GetNeighbors(CurrentNode);
//            foreach(Node neighbor in Neighbors)
//            {
//                TryAddOpenList(neighbor, CurrentNode);
//            }
//        }

//        return false;
//    }

//    List<Node> GetNeighbors(Node current)
//    {
//        List<Node> neighbors = new List<Node>();
//        for (int i = 1; i < (int)Direction.MaxValue; i++)
//        {
//            Vector2Int direction = DIRECTIONS[i];
//            if(direction == -current.SnapShotDirection) continue;
//            Node jumpPoint = Jump(current, direction);

//            if (jumpPoint != null && !jumpPoint.isWall)
//            {
//                neighbors.Add(jumpPoint); // 유효한 점프 포인트를 이웃으로 추가
//            }

//        }
//        return neighbors;
//    }

//    /// <summary>
//    /// 진행 가능 방향으로 코너가 나올 때까지 직진한다.
//    /// 재귀함수인거 고려하여 작성할 것.
//    /// </summary>
//    /// <param name="direction"></param>
//    /// <returns>
//    /// NULL : 탐색 중단
//    /// </returns>
//    Node Jump(Node nowNode, Vector2Int direction, bool recursive = true)
//    {      
//        // DIRECTIONS[(int)Direction.None]은 유효하지 않은 값이므로 제외. null 반환.
//        if (direction == DIRECTIONS[(int)Direction.None]) return null;

//        // direction대로 전진
//        Vector2Int checkPos = direction + new Vector2Int(nowNode.x, nowNode.y);

//        // 목표 지점 체크 : 좌표로만 노드의 동일성 비교를 하므로 direction과 무관계함.
//        if (checkPos == new Vector2Int(TargetNode.x, TargetNode.y))
//        {
//            return new Node(checkPos.x, checkPos.y, direction);
//        }

//        // 전진한 부분의 좌표가 벽일 경우
//        if (Physics2D.OverlapPoint(checkPos, LayerMask.GetMask("Wall")))
//        {
//            /*
//            //대각으로 가고 있었을 경우 대각 방향의 가로세로 성분으로 한번 Jump하고,
//            //둘 다 길이 없을 경우 그 전 노드를 리턴하도록 한다.
//            //만약 전 노드가 점프를 시작했던 노드라면, 
//            if(direction.x != 0 && direction.y != 0)
//            {
//                Vector2Int nextDirectionX = new Vector2Int(direction.x, 0);
//                Vector2Int nextDirectionY = new Vector2Int(0, direction.y);
//                Node newNode = new Node(checkPos.x, checkPos.y, direction);
//                if (Jump(newNode, nextDirectionX, false) == newNode)
//            }
//            else
//            {
//                //직선으로 가고 있었을 경우 recursive가 true면 
//                if (recursive)
//                {

//                }
//                else { }
//            }
//            */
//            bool isForcedX = Physics2D.OverlapPoint(new Vector2(nowNode.x + direction.x, nowNode.y), LayerMask.GetMask("Wall"));

//            bool isForcedY = Physics2D.OverlapPoint(new Vector2(nowNode.x, nowNode.y + direction.y), LayerMask.GetMask("Wall"));

//            if (isForcedX || isForcedY)
//            {
//                return new Node(checkPos.x, checkPos.y, direction);
//            }
//        }

//        //[TODO] : 동적 노드 생성이 필요한데, 이 경우는 점프하면서도 노드가 생성된다. direction대로 checkPos만 변경되도록 짜야한다.
//        // 직선 방향 직진
//        if (recursive)
//        {
//            Node nextNode = Jump(new Node(checkPos.x, checkPos.y, direction), direction);
//            if (nextNode != null)
//            {
//                return nextNode;
//            }
//        }
        
//        return new Node(checkPos.x, checkPos.y, direction);
//    }

//    void TryAddOpenList(Node neighbor, Node current)
//    {
//        //코스트 비교하기. 
//        // 현재 노드와 이웃 노드 사이의 거리 계산
//        int distance = Mathf.Abs(neighbor.x - current.x) + Mathf.Abs(neighbor.y - current.y);
//        int moveCost = (neighbor.SnapShotDirection.x != 0 && neighbor.SnapShotDirection.y != 0) ? DIAGONAL_COST : STRAIGHT_COST;

//        // G 값을 계산: 현재 노드에서 이웃 노드까지의 거리
//        int tentativeGScore = current.G + distance * moveCost;

//        // 이웃 노드가 이미 ClosedList에 있는지 확인
//        bool isInClosedList = ClosedList.Contains(neighbor);

//        // 더 나은 경로를 찾았을 때만 진행
//        if (tentativeGScore < neighbor.G || isInClosedList)
//        {
//            // 이웃 노드의 새로운 G 값을 업데이트하고 부모 설정
//            neighbor.G = tentativeGScore;
//            neighbor.H = Heuristic(new Vector2Int(neighbor.x, neighbor.y), new Vector2Int(TargetNode.x, TargetNode.y));
//            neighbor.ParentNode = current;

//            OpenList.Add(neighbor);
            
//        }
//    }

//    void ReconstructPath()
//    {
//        FinalNodeList = new List<Node>();
//        Node Target = TargetNode;
//        while (Target != StartNode)
//        {
//            FinalNodeList.Add(Target);
//            Target = Target.ParentNode;
//        }
//        FinalNodeList.Reverse();
//    } 

//    /// <summary>
//    /// 휴리스틱 함수. 맨해튼 거리보다 무조건 이하로 계산될 것이다.
//    /// </summary>
//    /// <param name="CurrNodePos">CurrentNode의 좌표다.</param>
//    /// <param name="EndPos">목표지점의 좌표다.</param>
//    /// <returns></returns>
//    private int Heuristic(Vector2Int CurrNodePos, Vector2Int EndPos)
//    {
//        int x = Mathf.Abs(CurrNodePos.x - EndPos.x);
//        int y = Mathf.Abs(CurrNodePos.y - EndPos.y);
//        int remain = Mathf.Abs(x - y);

//        return DIAGONAL_COST * Mathf.Min(x, y) + STRAIGHT_COST * remain;
//    }

//    void OnDrawGizmos()
//    {
//        if(FinalNodeList!= null)
//        {
//            if (FinalNodeList.Count != 0) for (int i = 0; i < FinalNodeList.Count - 1; i++)
//                    Gizmos.DrawLine(new Vector2(FinalNodeList[i].x, FinalNodeList[i].y), new Vector2(FinalNodeList[i + 1].x, FinalNodeList[i + 1].y));
//        }
        
//    }
//}

