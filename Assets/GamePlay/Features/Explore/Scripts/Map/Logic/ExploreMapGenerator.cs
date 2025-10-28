using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Core.Scripts.Foundation.Define;
using Core.Scripts.Foundation.Utils;
using GamePlay.Features.Explore.Scripts.Map.Data;

namespace GamePlay.Features.Explore.Scripts.Map.Logic
{
    public static class ExploreMapGenerator
    {
        // 튜닝 
        const float COVERAGE_MIN = 0.26f;              // 전체 셀 대비 최소 가공 비율
        const float COVERAGE_MAX = 0.48f;              // 전체 셀 대비 최대 가공 비율
        static readonly Vector2Int BRANCH_LEN_RANGE = new Vector2Int(2, 6);

        // 바깥 1타일 링은 항상 벽으로 남김
        const int BORDER = 1;

        // 출력 구조 
        public sealed class GeneratedMap
        {
            public int Width, Height;

            public readonly Dictionary<Vector2Int, SystemEnum.MapCellType> CellType
                = new Dictionary<Vector2Int, SystemEnum.MapCellType>(1024);

            public readonly Dictionary<Vector2Int, SystemEnum.CellEventType> EventAt
                = new Dictionary<Vector2Int, SystemEnum.CellEventType>(64);

            public readonly HashSet<Vector2Int> Walkable = new HashSet<Vector2Int>();

            public Vector2Int Start, Boss;

            public IEnumerable<Vector2Int> Items   => Filter(SystemEnum.MapCellType.Item);
            public IEnumerable<Vector2Int> Battles => Filter(SystemEnum.MapCellType.Battle);
            public IEnumerable<Vector2Int> Events  => Filter(SystemEnum.MapCellType.Event);

            IEnumerable<Vector2Int> Filter(SystemEnum.MapCellType t)
            {
                foreach (var kv in CellType) if (kv.Value == t) yield return kv.Key;
            }
        }

        // DB 주입
        static ExploreMapConfigDB _configDB;
        static ExploreEventDB     _eventDB;

        public static void SetConfigDB(ExploreMapConfigDB db) => _configDB = db;
        public static void SetEventDB(ExploreEventDB db)     => _eventDB  = db;

        static ExploreMapConfigDB GetConfigDB()
            => _configDB ? _configDB : (_configDB = Resources.Load<ExploreMapConfigDB>(SystemString.MapConfigDBPath));

        public static ExploreMap GenerateMap(SystemEnum.Dungeon dungeon, int floor, int seed)
        { GenerateRect(dungeon, floor, seed); return new ExploreMap(); }

        public static GeneratedMap GenerateRect(SystemEnum.Dungeon dungeon, int floor, int seed)
        {
            var cfgDB = GetConfigDB();
            var cfg   = cfgDB ? cfgDB.GetConfig(dungeon, floor) : null;
            if (!cfg) { Debug.LogError("[ExploreMapGenerator] Config not found"); return null; }

            // Seed 안정화(테스터 seed + dungeon + floor)
            ulong mix = RandomUtil.Mix3((ulong)seed, (ulong)(int)dungeon, (ulong)floor);
            var rng   = new GameRandom(mix);

            int W = Mathf.Max(6, cfg.xCapacity);
            int H = Mathf.Max(6, cfg.yCapacity);

            var res  = new GeneratedMap { Width = W, Height = H };
            var all  = BuildRectMask(W, H);

            foreach (var p in all) res.CellType[p] = SystemEnum.MapCellType.Wall;

            res.Start = PickFromQuadrantCornerInner(all, rng, 0, 0, W/2, H/2, new Vector2Int(BORDER, BORDER), W, H);
            res.Boss  = PickFromQuadrantCornerInner(all, rng, W/2, H/2, W,   H,   new Vector2Int(W-1-BORDER, H-1-BORDER), W, H);

            var trunk  = ConnectDirectZigzag(res.Start, res.Boss, W, H, rng);
            var carved = new HashSet<Vector2Int>(trunk);

            int total     = all.Count;
            int targetMin = Mathf.RoundToInt(total * COVERAGE_MIN);
            int targetMax = Mathf.RoundToInt(total * COVERAGE_MAX);

            int guard = 20000;
            var frontier = CollectFrontierStrict(carved, W, H, res.Start, res.Boss);
            while (carved.Count < targetMax && frontier.Count > 0 && guard-- > 0)
            {
                var seedP = PickFrontierBalanced(frontier, rng, res.Start, res.Boss);
                int len   = rng.Next(BRANCH_LEN_RANGE.x, BRANCH_LEN_RANGE.y + 1);
                CarveSinglePath(seedP, len, carved, W, H, rng);
                frontier = CollectFrontierStrict(carved, W, H, res.Start, res.Boss);
            }

            guard = 20000;
            while (carved.Count < targetMin && guard-- > 0)
            {
                frontier = CollectFrontierStrict(carved, W, H, res.Start, res.Boss);
                if (frontier.Count == 0) break;
                var seedP = PickFrontierBalanced(frontier, rng, res.Start, res.Boss);
                int len   = rng.Next(BRANCH_LEN_RANGE.x, BRANCH_LEN_RANGE.y + 1);
                CarveSinglePath(seedP, len, carved, W, H, rng);
            }

            // 5) 결과 반영
            carved.Add(res.Start);
            carved.Add(res.Boss);
            foreach (var p in carved)
            {
                res.Walkable.Add(p);
                res.CellType[p] = SystemEnum.MapCellType.Floor;
            }

            EnsureConnected(res, W, H, rng);
            EnforceOuterWallRing(res, W, H);
            SealDiagonalCorners(res, W, H);

            PlaceSymbols(cfg, rng, res);

            res.CellType[res.Start] = SystemEnum.MapCellType.StartPoint;
            res.CellType[res.Boss]  = SystemEnum.MapCellType.BossBattle;

            return res;
        }

        static readonly Vector2Int[] DIR4  = { new(1,0), new(-1,0), new(0,1), new(0,-1) };
        static readonly Vector2Int[] DIAG4 = { new(1,1), new(1,-1), new(-1,1), new(-1,-1) };

        static bool InBounds(Vector2Int p, int W, int H) => (uint)p.x < (uint)W && (uint)p.y < (uint)H;

        static bool InInnerBounds(Vector2Int p, int W, int H)
            => (uint)(p.x - BORDER) < (uint)(W - 2*BORDER)
            && (uint)(p.y - BORDER) < (uint)(H - 2*BORDER);

        static HashSet<Vector2Int> BuildRectMask(int W, int H)
        {
            var s = new HashSet<Vector2Int>(W * H);
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                    s.Add(new Vector2Int(x, y));
            return s;
        }


        // 내부 사분면 + 코너 근처에서 선택
        static Vector2Int PickFromQuadrantCornerInner(HashSet<Vector2Int> mask, GameRandom rng,
            int x0, int y0, int x1, int y1, Vector2Int corner, int W, int H)
        {
            var list = new List<Vector2Int>(64);
            foreach (var p in mask)
                if (p.x >= x0 && p.x < x1 && p.y >= y0 && p.y < y1 && InInnerBounds(p, W, H))
                    list.Add(p);

            if (list.Count == 0)
                return new Vector2Int(
                    Mathf.Clamp(corner.x, BORDER, W - 1 - BORDER),
                    Mathf.Clamp(corner.y, BORDER, H - 1 - BORDER)
                );

            list.Sort((a, b) =>
            {
                int da = (a.x - corner.x)*(a.x - corner.x) + (a.y - corner.y)*(a.y - corner.y);
                int db = (b.x - corner.x)*(b.x - corner.x) + (b.y - corner.y)*(b.y - corner.y);
                return da.CompareTo(db);
            });
            int take = Math.Max(1, list.Count * 3 / 10);
            return list[rng.Next(take)];
        }

        static List<Vector2Int> ConnectDirectZigzag(Vector2Int a, Vector2Int b, int W, int H, GameRandom rng)
        {
            var cur = a; var path = new List<Vector2Int> { a }; int guard = W * H;
            while (cur != b && guard-- > 0)
            {
                var cand = new List<Vector2Int>(2);
                if (b.x != cur.x) cand.Add(new Vector2Int(cur.x + Math.Sign(b.x - cur.x), cur.y));
                if (b.y != cur.y) cand.Add(new Vector2Int(cur.x, cur.y + Math.Sign(b.y - cur.y)));
                cand = cand.Where(p => InInnerBounds(p, W, H)).ToList();
                if (cand.Count == 0) break;
                cur = cand[rng.Next(cand.Count)];
                path.Add(cur);
            }
            return path;
        }

        static List<Vector2Int> ConnectDirectZigzagAvoid(Vector2Int a, Vector2Int b, int W, int H, GameRandom rng, HashSet<Vector2Int> forbidden)
        {
            var cur = a; var path = new List<Vector2Int> { a }; int guard = W * H;
            while (cur != b && guard-- > 0)
            {
                var cand = new List<Vector2Int>(2);
                if (b.x != cur.x) cand.Add(new Vector2Int(cur.x + Math.Sign(b.x - cur.x), cur.y));
                if (b.y != cur.y) cand.Add(new Vector2Int(cur.x, cur.y + Math.Sign(b.y - cur.y)));
                for (int i = cand.Count - 1; i >= 0; --i)
                    if (forbidden.Contains(cand[i]) || !InInnerBounds(cand[i], W, H))
                        cand.RemoveAt(i);
                if (cand.Count == 0) break;
                cur = cand[rng.Next(cand.Count)];
                path.Add(cur);
            }
            return path;
        }


        // 외길 제약: 인접한 Floor가 정확히 1개일 때만 가능
        static bool CanCarve(Vector2Int p, HashSet<Vector2Int> floor, int W, int H)
        {
            if (!InInnerBounds(p, W, H) || floor.Contains(p)) return false;
            int adj = 0;
            foreach (var d in DIR4) if (floor.Contains(p + d)) adj++;
            return adj == 1;
        }

        static List<Vector2Int> CollectFrontierStrict(HashSet<Vector2Int> floor, int W, int H, Vector2Int start, Vector2Int boss)
        {
            var list = new List<Vector2Int>(128);
            foreach (var f in floor)
            {
                if (f == start || f == boss) continue;
                foreach (var d in DIR4)
                {
                    var n = f + d;
                    if (!InInnerBounds(n, W, H) || floor.Contains(n)) continue;
                    if (CanCarve(n, floor, W, H)) list.Add(n);
                }
            }
            return list;
        }

        static Vector2Int PickFrontierBalanced(List<Vector2Int> frontier, GameRandom rng, Vector2Int start, Vector2Int boss, int k = 16)
        {
            if (frontier == null || frontier.Count == 0) return default;
            if (frontier.Count <= k) return frontier[rng.Next(frontier.Count)];

            Vector2Int best = frontier[rng.Next(frontier.Count)];
            int bestScore = -1;
            for (int i = 0; i < k; i++)
            {
                var p = frontier[rng.Next(frontier.Count)];
                int ds = Mathf.Abs(p.x - start.x) + Mathf.Abs(p.y - start.y);
                int db = Mathf.Abs(p.x - boss.x)  + Mathf.Abs(p.y - boss.y);
                int score = Math.Min(ds, db);
                if (score > bestScore) { bestScore = score; best = p; }
            }
            return best;
        }

        static void CarveSinglePath(Vector2Int seed, int len, HashSet<Vector2Int> floor, int W, int H, GameRandom rng)
        {
            var cur = seed; int steps = 0, guard = len * 3;
            while (steps < len && guard-- > 0)
            {
                if (!CanCarve(cur, floor, W, H)) break;
                floor.Add(cur); steps++;

                var cand = new List<Vector2Int>(4);
                foreach (var d in DIR4)
                {
                    var n = cur + d;
                    if (InInnerBounds(n, W, H) && !floor.Contains(n) && CanCarve(n, floor, W, H))
                        cand.Add(n);
                }
                if (cand.Count == 0) break;

                // 약한 바깥쪽 선호 + 소량 난수
                float Score(Vector2Int n)
                {
                    int cx = W / 2, cy = H / 2;
                    float outward = (Mathf.Abs(n.x - cx) + Mathf.Abs(n.y - cy))
                                  - (Mathf.Abs(cur.x - cx) + Mathf.Abs(cur.y - cy));
                    double r = rng.NextDouble();
                    return outward + (float)(0.25 * r);
                }
                cand.Sort((a, b) => Score(b).CompareTo(Score(a)));
                int take = Mathf.Max(1, cand.Count / 2);
                cur = cand[rng.Next(take)];
            }
        }


        static void PlaceSymbols(ExploreMapConfig cfg, GameRandom rng, GeneratedMap r)
        {
            var degree = new Dictionary<Vector2Int, int>(r.Walkable.Count);
            foreach (var p in r.Walkable)
            { int d = 0; foreach (var dir in DIR4) if (r.Walkable.Contains(p + dir)) d++; degree[p] = d; }

            var pool     = r.Walkable.Where(p => p != r.Start && p != r.Boss).ToList();
            var deadEnds = pool.Where(p => degree[p] <= 1).ToList();
            Shuffle(deadEnds, rng); Shuffle(pool, rng);
            int di = 0, pi = 0;

            // 이벤트 후보
            List<SystemEnum.CellEventType> evCands;
            if (cfg.eventCandidate != null && cfg.eventCandidate.Count > 0)
                evCands = cfg.eventCandidate.ToList();
            else if (_eventDB != null && _eventDB.symbols != null && _eventDB.symbols.Count > 0)
                evCands = _eventDB.symbols.Select(s => s.eventType).Distinct().ToList();
            else evCands = new List<SystemEnum.CellEventType>();

            // Event
            for (int i = 0; i < Math.Max(0, cfg.eventSymbolCount); i++)
            {
                var pos = (di < deadEnds.Count) ? deadEnds[di++] : (pi < pool.Count ? pool[pi++] : default);
                if (pos == default) break;
                var ev = (evCands.Count > 0) ? evCands[rng.Next(evCands.Count)]
                                             : default(SystemEnum.CellEventType);
                r.CellType[pos] = SystemEnum.MapCellType.Event;
                r.EventAt[pos]  = ev;
            }
            // Item
            for (int i = 0; i < Math.Max(0, cfg.itemSymbolCount); i++)
            {
                var pos = (di < deadEnds.Count) ? deadEnds[di++] : (pi < pool.Count ? pool[pi++] : default);
                if (pos == default) break;
                r.CellType[pos] = SystemEnum.MapCellType.Item;
            }
            // Battle
            for (int i = 0; i < Math.Max(0, cfg.battleSymbolCount); i++)
            {
                var pos = (di < deadEnds.Count) ? deadEnds[di++] : (pi < pool.Count ? pool[pi++] : default);
                if (pos == default) break;
                r.CellType[pos] = SystemEnum.MapCellType.Battle;
            }
        }


        static void EnsureConnected(GeneratedMap r, int W, int H, GameRandom rng)
        {
            var reached = new HashSet<Vector2Int>();
            var q = new Queue<Vector2Int>();
            q.Enqueue(r.Start); reached.Add(r.Start);

            while (q.Count > 0)
            {
                var p = q.Dequeue();
                foreach (var d in DIR4)
                {
                    var n = p + d;
                    if (!InInnerBounds(n, W, H) || !r.Walkable.Contains(n) || reached.Contains(n)) continue;
                    reached.Add(n); q.Enqueue(n);
                }
            }
            if (reached.SetEquals(r.Walkable)) return;

            var notReached = r.Walkable.Where(p => !reached.Contains(p)).ToList();
            while (notReached.Count > 0)
            {
                var a = notReached[0];
                Vector2Int best = default; int bestD = int.MaxValue;
                foreach (var b in reached)
                {
                    if (b == r.Boss || b == r.Start) continue; // 특수노드 회피
                    int d = Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
                    if (d < bestD) { best = b; bestD = d; }
                }
                if (best == default) break;

                var forbidden = new HashSet<Vector2Int> { r.Start, r.Boss };
                var path = ConnectDirectZigzagAvoid(a, best, W, H, rng, forbidden);
                foreach (var p in path)
                {
                    r.Walkable.Add(p);
                    r.CellType[p] = SystemEnum.MapCellType.Floor;
                    reached.Add(p);
                }
                notReached = r.Walkable.Where(p => !reached.Contains(p)).ToList();
            }
        }

        // 바깥 타일 그냥 다 둘러쌈
        static void EnforceOuterWallRing(GeneratedMap r, int W, int H)
        {
            for (int x = 0; x < W; x++)
            {
                var a = new Vector2Int(x, 0);
                var b = new Vector2Int(x, H - 1);
                r.Walkable.Remove(a); r.CellType[a] = SystemEnum.MapCellType.Wall;
                r.Walkable.Remove(b); r.CellType[b] = SystemEnum.MapCellType.Wall;
            }
            for (int y = 0; y < H; y++)
            {
                var a = new Vector2Int(0, y);
                var b = new Vector2Int(W - 1, y);
                r.Walkable.Remove(a); r.CellType[a] = SystemEnum.MapCellType.Wall;
                r.Walkable.Remove(b); r.CellType[b] = SystemEnum.MapCellType.Wall;
            }
        }
        
        static bool IsStartBossConnected(GeneratedMap r, int W, int H)
        {
            var visited = new HashSet<Vector2Int>();
            var q = new Queue<Vector2Int>();
            q.Enqueue(r.Start);
            visited.Add(r.Start);

            while (q.Count > 0)
            {
                var p = q.Dequeue();
                if (p == r.Boss) return true;
                foreach (var d in DIR4)
                {
                    var n = p + d;
                    if ((uint)n.x >= (uint)W || (uint)n.y >= (uint)H) continue;
                    if (!r.Walkable.Contains(n) || visited.Contains(n)) continue;
                    visited.Add(n);
                    q.Enqueue(n);
                }
            }
            return false;
        }

        static bool SafeWallify(GeneratedMap r, Vector2Int cell, int W, int H)
        {
            if (!r.Walkable.Contains(cell)) return false;
            if (cell == r.Start || cell == r.Boss) return false;

            // 임시로 벽으로 전환
            var prevType = r.CellType[cell];
            r.Walkable.Remove(cell);
            r.CellType[cell] = SystemEnum.MapCellType.Wall;

            bool ok = IsStartBossConnected(r, W, H);
            if (!ok)
            {
                // 롤백
                r.Walkable.Add(cell);
                r.CellType[cell] = prevType;
            }
            return ok;
        }
        
        static void SealDiagonalCorners(GeneratedMap r, int W, int H)
        {
            int Degree(Vector2Int p)
            {
                int c = 0;
                foreach (var d in DIR4)
                {
                    var n = p + d;
                    if ((uint)n.x < (uint)W && (uint)n.y < (uint)H && r.Walkable.Contains(n)) c++;
                }

                return c;
            }

            bool IsProtected(Vector2Int p) => p == r.Start || p == r.Boss;

            var toTry = new List<(Vector2Int p, Vector2Int q)>(128);
            bool changed;
            int guard = W * H;

            do
            {
                changed = false;
                toTry.Clear();

                // 대각으로만 맞닿은 Floor 페어 수집
                foreach (var p in r.Walkable)
                {
                    foreach (var d in DIAG4)
                    {
                        var q = p + d;
                        if (!((uint)q.x < (uint)W && (uint)q.y < (uint)H)) continue;
                        if (!r.Walkable.Contains(q)) continue;

                        // 대각 사이의 직교 셀
                        var a = new Vector2Int(p.x + d.x, p.y);
                        var b = new Vector2Int(p.x, p.y + d.y);

                        bool aWall = !((uint)a.x < (uint)W && (uint)a.y < (uint)H) || !r.Walkable.Contains(a);
                        bool bWall = !((uint)b.x < (uint)W && (uint)b.y < (uint)H) || !r.Walkable.Contains(b);

                        if (aWall && bWall) // "대각으로만" 붙어있는 상황
                        {
                            // 보호 노드 제외
                            if (IsProtected(p) || IsProtected(q)) continue;
                            // 한 번만 다루기 위해 "정렬" (중복 방지)
                            if (p.x < q.x || (p.x == q.x && p.y < q.y))
                                toTry.Add((p, q));
                        }
                    }
                }

                // 시도
                foreach (var (p, q) in toTry)
                {
                    if (!r.Walkable.Contains(p) || !r.Walkable.Contains(q)) continue; // 이전 루프에서 바뀌었을 수 있음

                    int dp = Degree(p), dq = Degree(q);

                    // 1) 리프 우선 제거
                    if (dp <= 1 && SafeWallify(r, p, W, H))
                    {
                        changed = true;
                        continue;
                    }

                    if (dq <= 1 && SafeWallify(r, q, W, H))
                    {
                        changed = true;
                        continue;
                    }

                    // 2) 둘 다 리프가 아니면, 연결성 유지 범위에서 더 '약한' 쪽 제거(차수 낮은 쪽 우선)
                    if (dp <= dq)
                    {
                        if (SafeWallify(r, p, W, H))
                        {
                            changed = true;
                            continue;
                        }

                        if (SafeWallify(r, q, W, H))
                        {
                            changed = true;
                            continue;
                        }
                    }
                    else
                    {
                        if (SafeWallify(r, q, W, H))
                        {
                            changed = true;
                            continue;
                        }

                        if (SafeWallify(r, p, W, H))
                        {
                            changed = true;
                            continue;
                        }
                    }

                    // 3) 둘 다 제거하면 Start-Boss가 끊기는 경우 → 유지(스킵)
                }
            } while (changed && guard-- > 0);
        }


        static void Shuffle<T>(IList<T> list, GameRandom rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            { int j = rng.Next(i + 1); (list[i], list[j]) = (list[j], list[i]); }
        }
    }
}
