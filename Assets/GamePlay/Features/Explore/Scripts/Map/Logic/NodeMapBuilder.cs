using Core.Scripts.Foundation.Define;
using Core.Scripts.Foundation.Utils;
using GamePlay.Features.Explore.Scripts.Map.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GamePlay.Features.Explore.Scripts.Map.Logic
{
    /// <summary>
    /// 노드 기반 맵 생성기
    /// </summary>
    public class NodeMapBuilder
    {
        private readonly ExploreMapConfig _cfg;
        private readonly GameRandom _rng;
        private readonly ExploreMapSkeleton _skel;

        private bool[] _interiorMask;
        private List<(int x, int y)> _interiorCells;

        private List<Node> _nodes = new();
        private (int x, int y) _anchorA;
        private (int x, int y) _anchorB;

        private class Node
        {
            public int X, Y;              
            public int Radius;            
            public List<Node> Connections = new();
            
            public SystemEnum.MapSymbolType? SymbolType;
            public SystemEnum.CellEventType? EventType;
            public long? ItemIndex;
        }

        public NodeMapBuilder(ExploreMapConfig cfg, int seed)
        {
            _cfg = cfg;
            _rng = new GameRandom((ulong)seed);
            _skel = new ExploreMapSkeleton(
                dungeonName: cfg.dungeonName.ToString(),
                floor: cfg.floor,
                width: cfg.xCapacity,
                height: cfg.yCapacity,
                seed: seed
            );
        }

        #region Core Methods

        public void ComputeInteriorMask()
        {
            int W = _skel.Width;
            int H = _skel.Height;
            _interiorMask = new bool[W * H];
            _interiorCells = new List<(int, int)>(W * H / 2 + 8);

            int cx = (W - 1) / 2;
            int cy = (H - 1) / 2;
            int hx = Math.Max(1, (W - 1) / 2);
            int hy = Math.Max(1, (H - 1) / 2);

            for (int y = 0; y < H; y++)
            {
                int dy = Math.Abs(y - cy);
                for (int x = 0; x < W; x++)
                {
                    int dx = Math.Abs(x - cx);
                    bool isInside = (dx * hy + dy * hx) <= (hx * hy);

                    if (x == 0 || y == 0 || x == W - 1 || y == H - 1)
                        isInside = false;

                    if (isInside)
                    {
                        _interiorMask[Index(x, y)] = true;
                        _interiorCells.Add((x, y));
                    }
                }
            }

            if (_interiorCells.Count == 0)
                throw new InvalidOperationException("Interior mask is empty.");
        }

        public void InitWalls()
        {
            for (int y = 0; y < _skel.Height; y++)
                for (int x = 0; x < _skel.Width; x++)
                    _skel.SetCellType(x, y, SystemEnum.MapCellType.Wall);
        }

        public void PlaceNodes(int targetCount = 12, int minDistance = 5)
        {
            var candidates = new List<(int x, int y)>(_interiorCells);
            int attempts = 0;
            const int MAX_ATTEMPTS = 1000;

            while (_nodes.Count < targetCount && attempts < MAX_ATTEMPTS)
            {
                attempts++;
                if (candidates.Count == 0) break;

                var pos = candidates[_rng.Next(candidates.Count)];
                
                bool tooClose = false;
                foreach (var node in _nodes)
                {
                    int dist = Math.Abs(pos.x - node.X) + Math.Abs(pos.y - node.Y);
                    if (dist < minDistance)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (!tooClose)
                {
                    var node = new Node
                    {
                        X = pos.x,
                        Y = pos.y,
                        Radius = _rng.Next(1, 3)
                    };
                    _nodes.Add(node);
                    
                    candidates.RemoveAll(c => 
                        Math.Abs(c.x - pos.x) < minDistance && 
                        Math.Abs(c.y - pos.y) < minDistance
                    );
                }
                else
                {
                    candidates.Remove(pos);
                }
            }

            if (_nodes.Count < 2)
                throw new InvalidOperationException("Failed to place enough nodes.");
        }

        /// <summary>
        /// [수정] 재시도 10회 + Fallback 전략
        /// </summary>
        public void ConnectNodesWithMST()
        {
            if (_nodes.Count < 2) return;

            const int MAX_RETRIES = 10;
            bool useInteriorOnly = true;

            for (int retry = 0; retry < MAX_RETRIES; retry++)
            {
                // 5회 실패 시 interior 제약 완화
                if (retry >= 5)
                    useInteriorOnly = false;

                // MST 구성
                foreach (var node in _nodes) node.Connections.Clear();
                
                var connected = new HashSet<Node> { _nodes[0] };
                var unconnected = new HashSet<Node>(_nodes.Skip(1));

                while (unconnected.Count > 0)
                {
                    Node bestFrom = null;
                    Node bestTo = null;
                    int minDist = int.MaxValue;

                    foreach (var from in connected)
                    {
                        foreach (var to in unconnected)
                        {
                            int dist = Math.Abs(from.X - to.X) + Math.Abs(from.Y - to.Y);
                            if (dist < minDist)
                            {
                                minDist = dist;
                                bestFrom = from;
                                bestTo = to;
                            }
                        }
                    }

                    if (bestTo != null)
                    {
                        bestFrom.Connections.Add(bestTo);
                        bestTo.Connections.Add(bestFrom);
                        connected.Add(bestTo);
                        unconnected.Remove(bestTo);
                    }
                }

                // 복도 파기
                CarveCorridors(useInteriorOnly);

                // 검증
                if (AllNodesReachable())
                {
                    // 대각선 연결 제거
                    FixDiagonalGaps();
                    return;
                }
            }

            throw new InvalidOperationException($"Failed to connect all nodes after {MAX_RETRIES} retries.");
        }

        public void AddExtraConnections(int count)
        {
            for (int i = 0; i < count && _nodes.Count > 1; i++)
            {
                var nodeA = _nodes[_rng.Next(_nodes.Count)];
                var nodeB = _nodes[_rng.Next(_nodes.Count)];

                if (nodeA != nodeB && !nodeA.Connections.Contains(nodeB))
                {
                    int dist = Math.Abs(nodeA.X - nodeB.X) + Math.Abs(nodeA.Y - nodeB.Y);
                    if (dist < _skel.Width / 2)
                    {
                        nodeA.Connections.Add(nodeB);
                        nodeB.Connections.Add(nodeA);
                        
                        CarveCorridor(nodeA.X, nodeA.Y, nodeB.X, nodeB.Y, useInteriorOnly: true);
                        FixDiagonalGaps();
                    }
                }
            }
        }

        public void CarveNodes()
        {
            foreach (var node in _nodes)
            {
                if (_interiorMask[Index(node.X, node.Y)])
                    _skel.SetCellType(node.X, node.Y, SystemEnum.MapCellType.Floor);

                for (int dy = -node.Radius; dy <= node.Radius; dy++)
                {
                    for (int dx = -node.Radius; dx <= node.Radius; dx++)
                    {
                        if (Math.Abs(dx) + Math.Abs(dy) <= node.Radius)
                        {
                            int nx = node.X + dx;
                            int ny = node.Y + dy;
                            if (_skel.InBounds(nx, ny) && _interiorMask[Index(nx, ny)])
                            {
                                _skel.SetCellType(nx, ny, SystemEnum.MapCellType.Floor);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// [수정] useInteriorOnly 파라미터 추가
        /// </summary>
        public void CarveCorridors(bool useInteriorOnly = true)
        {
            var processedPairs = new HashSet<(Node, Node)>();

            foreach (var node in _nodes)
            {
                foreach (var connected in node.Connections)
                {
                    var pair = node.GetHashCode() < connected.GetHashCode() 
                        ? (node, connected) 
                        : (connected, node);

                    if (processedPairs.Contains(pair)) continue;
                    processedPairs.Add(pair);

                    CarveCorridor(node.X, node.Y, connected.X, connected.Y, useInteriorOnly);
                }
            }
        }

        /// <summary>
        /// [완전 재작성] BFS 기반 복도 생성 + Fallback
        /// </summary>
        private void CarveCorridor(int x1, int y1, int x2, int y2, bool useInteriorOnly)
        {
            List<(int x, int y)> path = null;

            if (useInteriorOnly)
            {
                // 시도 1: BFS로 interior 내부에서만 경로 찾기
                path = FindPathInInterior((x1, y1), (x2, y2));
            }

            if (path == null || path.Count == 0)
            {
                // Fallback: 강제 L자 복도 (interior 무시)
                CarveCorridorFallback(x1, y1, x2, y2);
                return;
            }

            // 경로를 따라 Floor로 파기
            foreach (var (x, y) in path)
            {
                if (_skel.InBounds(x, y))
                    _skel.SetCellType(x, y, SystemEnum.MapCellType.Floor);
            }
        }

        /// <summary>
        /// [신규] BFS로 interior 내부에서만 최단 경로 찾기
        /// </summary>
        private List<(int x, int y)> FindPathInInterior((int x, int y) start, (int x, int y) end)
        {
            int W = _skel.Width, H = _skel.Height;
            var q = new Queue<(int x, int y)>();
            var prev = new Dictionary<(int, int), (int, int)>();
            var visited = new bool[W * H];

            if (!_interiorMask[Index(start.x, start.y)] || !_interiorMask[Index(end.x, end.y)])
                return null;

            q.Enqueue(start);
            visited[Index(start.x, start.y)] = true;
            prev[start] = (-1, -1);

            while (q.Count > 0)
            {
                var cur = q.Dequeue();
                if (cur == end) break;

                foreach (var (nx, ny) in GetNeighbors4(cur.x, cur.y))
                {
                    int ni = Index(nx, ny);
                    if (!_interiorMask[ni] || visited[ni]) continue;
                    
                    visited[ni] = true;
                    prev[(nx, ny)] = cur;
                    q.Enqueue((nx, ny));
                }
            }

            // 경로 역추적
            if (!visited[Index(end.x, end.y)]) return null;

            var path = new List<(int x, int y)>();
            var p = end;
            while (p.x != -1)
            {
                path.Add(p);
                p = prev[p];
            }
            path.Reverse();
            return path;
        }

        /// <summary>
        /// [신규] Fallback: interior 무시 강제 L자 복도
        /// </summary>
        private void CarveCorridorFallback(int x1, int y1, int x2, int y2)
        {
            bool xFirst = _rng.Next(2) == 0;

            if (xFirst)
            {
                // X축 먼저
                int x = x1;
                while (x != x2)
                {
                    if (_skel.InBounds(x, y1))
                        _skel.SetCellType(x, y1, SystemEnum.MapCellType.Floor);
                    x += Math.Sign(x2 - x);
                }

                // Y축
                int y = y1;
                while (y != y2)
                {
                    if (_skel.InBounds(x2, y))
                        _skel.SetCellType(x2, y, SystemEnum.MapCellType.Floor);
                    y += Math.Sign(y2 - y);
                }
            }
            else
            {
                // Y축 먼저
                int y = y1;
                while (y != y2)
                {
                    if (_skel.InBounds(x1, y))
                        _skel.SetCellType(x1, y, SystemEnum.MapCellType.Floor);
                    y += Math.Sign(y2 - y);
                }

                // X축
                int x = x1;
                while (x != x2)
                {
                    if (_skel.InBounds(x, y2))
                        _skel.SetCellType(x, y2, SystemEnum.MapCellType.Floor);
                    x += Math.Sign(x2 - x);
                }
            }

            // 도착점 보장
            if (_skel.InBounds(x2, y2))
                _skel.SetCellType(x2, y2, SystemEnum.MapCellType.Floor);
        }

        /// <summary>
        /// [신규] 대각선 연결 완전 차단 - 2x2 체커보드 패턴 제거
        /// </summary>
        private void FixDiagonalGaps()
        {
            int W = _skel.Width, H = _skel.Height;

            for (int y = 0; y < H - 1; y++)
            {
                for (int x = 0; x < W - 1; x++)
                {
                    var tl = _skel.GetCellType(x, y);
                    var tr = _skel.GetCellType(x + 1, y);
                    var bl = _skel.GetCellType(x, y + 1);
                    var br = _skel.GetCellType(x + 1, y + 1);

                    // 대각선 연결 패턴:
                    // F W    or    W F
                    // W F          F W
                    bool isDiagonal1 = (tl == SystemEnum.MapCellType.Floor && br == SystemEnum.MapCellType.Floor &&
                                        tr == SystemEnum.MapCellType.Wall && bl == SystemEnum.MapCellType.Wall);
                    bool isDiagonal2 = (tr == SystemEnum.MapCellType.Floor && bl == SystemEnum.MapCellType.Floor &&
                                        tl == SystemEnum.MapCellType.Wall && br == SystemEnum.MapCellType.Wall);

                    if (isDiagonal1)
                    {
                        // interior 우선으로 1칸 열기
                        if (_interiorMask[Index(x + 1, y)])
                            _skel.SetCellType(x + 1, y, SystemEnum.MapCellType.Floor);
                        else if (_interiorMask[Index(x, y + 1)])
                            _skel.SetCellType(x, y + 1, SystemEnum.MapCellType.Floor);
                        else // 둘 다 밖이면 아무거나
                            _skel.SetCellType(x + 1, y, SystemEnum.MapCellType.Floor);
                    }
                    else if (isDiagonal2)
                    {
                        if (_interiorMask[Index(x, y)])
                            _skel.SetCellType(x, y, SystemEnum.MapCellType.Floor);
                        else if (_interiorMask[Index(x + 1, y + 1)])
                            _skel.SetCellType(x + 1, y + 1, SystemEnum.MapCellType.Floor);
                        else
                            _skel.SetCellType(x, y, SystemEnum.MapCellType.Floor);
                    }
                }
            }
        }

        private bool AllNodesReachable()
        {
            if (_nodes.Count < 2) return true;

            var start = (_nodes[0].X, _nodes[0].Y);
            var reachable = FloodFillFloors(start);

            foreach (var node in _nodes)
            {
                if (!reachable[Index(node.X, node.Y)])
                    return false;
            }
            return true;
        }

        private bool[] FloodFillFloors((int x, int y) start)
        {
            int W = _skel.Width, H = _skel.Height;
            var visited = new bool[W * H];
            var q = new Queue<(int x, int y)>();

            if (!_skel.IsFloor(start.x, start.y)) return visited;

            q.Enqueue(start);
            visited[Index(start.x, start.y)] = true;

            while (q.Count > 0)
            {
                var p = q.Dequeue();
                foreach (var n in _skel.GetNeighbors4(p.x, p.y))
                {
                    int ni = Index(n.x, n.y);
                    if (visited[ni]) continue;
                    if (!_skel.IsFloor(n.x, n.y)) continue;
                    visited[ni] = true;
                    q.Enqueue(n);
                }
            }
            return visited;
        }

        public void ChooseAnchors()
        {
            if (_nodes.Count < 2)
                throw new InvalidOperationException("Not enough nodes.");

            Node furthestA = _nodes[0], furthestB = _nodes[1];
            int maxDist = 0;

            for (int i = 0; i < _nodes.Count; i++)
            {
                for (int j = i + 1; j < _nodes.Count; j++)
                {
                    int dist = Math.Abs(_nodes[i].X - _nodes[j].X) + 
                               Math.Abs(_nodes[i].Y - _nodes[j].Y);
                    if (dist > maxDist)
                    {
                        maxDist = dist;
                        furthestA = _nodes[i];
                        furthestB = _nodes[j];
                    }
                }
            }

            _anchorA = (furthestA.X, furthestA.Y);
            _anchorB = (furthestB.X, furthestB.Y);

            furthestA.SymbolType = SystemEnum.MapSymbolType.StartPoint;
            furthestB.SymbolType = SystemEnum.MapSymbolType.EndPoint;
        }

        public void SealBorderWalls(int thickness = 1)
        {
            int W = _skel.Width, H = _skel.Height;
            thickness = Math.Max(1, Math.Min(thickness, Math.Min(W, H) / 2));

            for (int t = 0; t < thickness; t++)
            {
                for (int x = 0; x < W; x++)
                {
                    _skel.SetCellType(x, t, SystemEnum.MapCellType.Wall);
                    _skel.SetCellType(x, H - 1 - t, SystemEnum.MapCellType.Wall);
                }
                for (int y = 0; y < H; y++)
                {
                    _skel.SetCellType(t, y, SystemEnum.MapCellType.Wall);
                    _skel.SetCellType(W - 1 - t, y, SystemEnum.MapCellType.Wall);
                }
            }
        }

        public void ScatterSymbols()
        {
            if (_cfg.symbolConfig == null || _cfg.symbolConfig.Count == 0) return;

            float[] eventWeights = _cfg.eventCandidate?.Select(e => (float)Math.Max(0, e.probability)).ToArray();
            float[] itemWeights = _cfg.itemCandidate?.Select(i => (float)Math.Max(0, i.dropProbability)).ToArray();

            var startNode = _nodes.FirstOrDefault(n => n.SymbolType == SystemEnum.MapSymbolType.StartPoint);
            var endNode = _nodes.FirstOrDefault(n => n.SymbolType == SystemEnum.MapSymbolType.EndPoint);

            if (startNode != null)
            {
                if (!_skel.TryAddSimpleSymbol(startNode.X, startNode.Y, SystemEnum.MapSymbolType.StartPoint, out _, out string err))
                    throw new InvalidOperationException($"Failed to place StartPoint: {err}");
            }

            if (endNode != null)
            {
                if (!_skel.TryAddSimpleSymbol(endNode.X, endNode.Y, SystemEnum.MapSymbolType.EndPoint, out _, out string err))
                    throw new InvalidOperationException($"Failed to place EndPoint: {err}");
            }

            var availableNodes = _nodes.Where(n => n.SymbolType == null).ToList();

            foreach (var entry in _cfg.symbolConfig)
            {
                var type = entry.symbolType;
                if (type == SystemEnum.MapSymbolType.StartPoint || type == SystemEnum.MapSymbolType.EndPoint)
                    continue;

                int need = Math.Max(0, entry.symbolCount);
                int placed = 0;

                const int MAX_TRIES_PER_SYMBOL = 100;

                for (int i = 0; i < need; i++)
                {
                    bool success = false;

                    for (int attempt = 0; attempt < MAX_TRIES_PER_SYMBOL && !success; attempt++)
                    {
                        if (availableNodes.Count == 0)
                        {
                            UnityEngine.Debug.LogWarning($"[NodeMapBuilder] No available nodes left for symbol {type}. Placed {placed}/{need}.");
                            break;
                        }

                        var node = availableNodes[_rng.Next(availableNodes.Count)];

                        if (type == SystemEnum.MapSymbolType.Event)
                        {
                            if (eventWeights != null && eventWeights.Length > 0)
                            {
                                int idx = _rng.WeightedChoice(eventWeights);
                                var et = _cfg.eventCandidate[idx].eventType;
                                if (_skel.TryAddEventSymbol(node.X, node.Y, et, out _, out _))
                                {
                                    availableNodes.Remove(node);
                                    placed++;
                                    success = true;
                                }
                            }
                        }
                        else if (type == SystemEnum.MapSymbolType.Item)
                        {
                            if (itemWeights != null && itemWeights.Length > 0)
                            {
                                int idx = _rng.WeightedChoice(itemWeights);
                                long itemIndex = _cfg.itemCandidate[idx].itemIndex;
                                if (_skel.TryAddItemSymbol(node.X, node.Y, itemIndex, out _, out _))
                                {
                                    availableNodes.Remove(node);
                                    placed++;
                                    success = true;
                                }
                            }
                        }
                        else
                        {
                            if (_skel.TryAddSimpleSymbol(node.X, node.Y, type, out _, out _))
                            {
                                availableNodes.Remove(node);
                                placed++;
                                success = true;
                            }
                        }
                    }

                    if (!success)
                    {
                        UnityEngine.Debug.LogWarning($"[NodeMapBuilder] Failed to place symbol {type} after {MAX_TRIES_PER_SYMBOL} attempts. Placed {placed}/{need}.");
                    }
                }

                if (placed < need)
                {
                    UnityEngine.Debug.LogWarning($"[NodeMapBuilder] Only placed {placed}/{need} symbols of type {type}.");
                }
            }
        }

        public void BasicValidate()
        {
            int floorCount = 0;
            for (int y = 0; y < _skel.Height; y++)
                for (int x = 0; x < _skel.Width; x++)
                    if (_skel.IsFloor(x, y)) floorCount++;

            if (floorCount == 0)
                throw new InvalidOperationException("No floor cells.");

            var starts = _skel.GetSymbolsOfType(SystemEnum.MapSymbolType.StartPoint).ToList();
            var ends = _skel.GetSymbolsOfType(SystemEnum.MapSymbolType.EndPoint).ToList();
            if (starts.Count > 0 && ends.Count > 0)
            {
                var s0 = (starts[0].X, starts[0].Y);
                var e0 = (ends[0].X, ends[0].Y);
                if (!ReachableOnFloors(s0, e0))
                    throw new InvalidOperationException("No path between Start and End.");
            }
        }

        public ExploreMapSkeleton ToSkeleton() => _skel;

        #endregion

        #region Utility

        private int Index(int x, int y) => y * _skel.Width + x;

        private IEnumerable<(int x, int y)> GetNeighbors4(int x, int y)
        {
            if (x > 0) yield return (x - 1, y);
            if (x + 1 < _skel.Width) yield return (x + 1, y);
            if (y > 0) yield return (x, y - 1);
            if (y + 1 < _skel.Height) yield return (x, y + 1);
        }

        private bool ReachableOnFloors((int x, int y) A, (int x, int y) B)
        {
            int W = _skel.Width, H = _skel.Height;
            var q = new Queue<(int x, int y)>();
            var vis = new bool[W * H];
            if (!_skel.IsFloor(A.x, A.y)) return false;

            q.Enqueue(A);
            vis[Index(A.x, A.y)] = true;

            while (q.Count > 0)
            {
                var p = q.Dequeue();
                if (p.x == B.x && p.y == B.y) return true;

                foreach (var n in _skel.GetNeighbors4(p.x, p.y))
                {
                    int ni = Index(n.x, n.y);
                    if (vis[ni]) continue;
                    if (!_skel.IsFloor(n.x, n.y)) continue;
                    vis[ni] = true;
                    q.Enqueue(n);
                }
            }
            return false;
        }

        #endregion
    }
}