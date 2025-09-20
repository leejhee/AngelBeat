using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Core.Scripts.Foundation.Define;
using Core.Scripts.Foundation.Utils;
using GamePlay.Features.Explore.Scripts.Map.Data;

namespace GamePlay.Features.Explore.Scripts.Map.Logic
{
    /// <summary>
    /// 직사각 영역(x*y)을 전부 Wall로 채운 뒤, 트렁크+브랜치로 "외길"만 카빙하는 절차 생성기.
    /// - Start: 좌하단 사분면, Boss: 우상단 사분면.
    /// - Trunk: Start→Boss 지그재그 직결(항상 연결).
    /// - Branch: 전역 프런티어에서 무작위 선택 + CanCarve(인접 Floor 1칸) 제약 → 외길/루프 방지.
    /// - Event 후보: config.eventCandidate 우선, 없으면 ExploreEventDB의 키 사용.
    /// </summary>
    public static class ExploreMapGenerator
    {
        // ── 튜닝 ──────────────────────────────────────────────────────
        // 목표 커버리지(전체 칸 대비 Walkable 비율)
        const float COVERAGE_MIN = 0.26f;
        const float COVERAGE_MAX = 0.48f;
        // 브랜치 길이 범위(외길 카빙)
        static readonly Vector2Int BRANCH_LEN_RANGE = new Vector2Int(2, 6);

        // ── 결과 ────────────────────────────────────────────────────
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

        // ── DB 주입 ─────────────────────────────────────────────────
        static ExploreMapConfigDB _configDB;
        static ExploreEventDB     _eventDB;

        public static void SetConfigDB(ExploreMapConfigDB db) => _configDB = db;
        public static void SetEventDB(ExploreEventDB db)     => _eventDB  = db;

        static ExploreMapConfigDB GetConfigDB()
            => _configDB ? _configDB : (_configDB = Resources.Load<ExploreMapConfigDB>(SystemString.MapConfigDBPath));

        // 구(호환) 시그니처
        public static ExploreMap GenerateMap(SystemEnum.Dungeon dungeon, int floor, int seed)
        { GenerateRect(dungeon, floor, seed); return new ExploreMap(); }

        // ── 메인 ────────────────────────────────────────────────────
        public static GeneratedMap GenerateRect(SystemEnum.Dungeon dungeon, int floor, int seed)
        {
            var cfgDB = GetConfigDB();
            var cfg   = cfgDB ? cfgDB.GetConfig(dungeon, floor) : null;
            if (!cfg) { Debug.LogError("[MapGen] Config not found"); return null; }

            // Seed 안정화(테스터 seed + dungeon + floor)
            ulong mix = RandomUtil.Mix3((ulong)seed, (ulong)(int)dungeon, (ulong)floor);
            var rng   = new GameRandom(mix);

            int W = Mathf.Max(6, cfg.xCapacity);
            int H = Mathf.Max(6, cfg.yCapacity);

            var res  = new GeneratedMap { Width = W, Height = H };
            var all  = BuildRectMask(W, H); // 전체 영역

            // 0) 전 칸 Wall
            foreach (var p in all) res.CellType[p] = SystemEnum.MapCellType.Wall;

            // 1) 사분면 선택
            res.Start = PickFromQuadrant(all, rng, 0, 0, W/2, H/2);
            res.Boss  = PickFromQuadrant(all, rng, W/2, H/2, W, H);

            // 2) 트렁크: 지그재그 직결(항상 연결)
            var trunk = ConnectDirectZigzag(res.Start, res.Boss, W, H, rng);
            var f = new HashSet<Vector2Int>(trunk);

            // 3) 프런티어 기반 브랜치(외길 보장 + 중앙 편중 완화)
            int total     = all.Count;                    // = W*H
            int targetMin = Mathf.RoundToInt(total * COVERAGE_MIN);
            int targetMax = Mathf.RoundToInt(total * COVERAGE_MAX);

            var guard = 20000;
            var frontier = CollectFrontierStrict(f, W, H);
            while (f.Count < targetMax && frontier.Count > 0 && guard-- > 0)
            {
                var seedP = frontier[rng.Next(frontier.Count)]; // 전역에서 랜덤 픽 → 편중 완화
                int len   = rng.Next(BRANCH_LEN_RANGE.x, BRANCH_LEN_RANGE.y + 1);
                CarveSinglePath(seedP, len, f, W, H, rng);
                frontier = CollectFrontierStrict(f, W, H);
            }

            // 커버리지 하한 보정(여전히 프런티어만 사용 → 항상 연결)
            guard = 20000;
            while (f.Count < targetMin && guard-- > 0)
            {
                frontier = CollectFrontierStrict(f, W, H);
                if (frontier.Count == 0) break;
                var seedP = frontier[rng.Next(frontier.Count)];
                CarveSinglePath(seedP, rng.Next(2, 5), f, W, H, rng);
            }

            // 4) 결과 반영 + Start/Boss 보장
            f.Add(res.Start);
            f.Add(res.Boss);

            foreach (var p in f)
            {
                res.CellType[p] = SystemEnum.MapCellType.Floor;
                res.Walkable.Add(p);
            }
            res.CellType[res.Start] = SystemEnum.MapCellType.StartPoint;
            res.CellType[res.Boss]  = SystemEnum.MapCellType.BossBattle;

            // 5) 심볼 배치(막다른 길 우선)
            PlaceSymbols(cfg, rng, res);

            // 6) 최종 연결성 보장(안전망)
            EnsureConnected(res, W, H, rng);

            return res;
        }

        // ── 유틸 ────────────────────────────────────────────────────
        static readonly Vector2Int[] DIR4 = { new(0,1), new(1,0), new(0,-1), new(-1,0) };

        static bool InBounds(Vector2Int p, int W, int H) => (uint)p.x < (uint)W && (uint)p.y < (uint)H;

        static HashSet<Vector2Int> BuildRectMask(int W, int H)
        {
            var s = new HashSet<Vector2Int>(W * H);
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                    s.Add(new Vector2Int(x, y));
            return s;
        }

        static Vector2Int PickFromQuadrant(HashSet<Vector2Int> mask, GameRandom rng, int x0, int y0, int x1, int y1)
        {
            var list = new List<Vector2Int>(64);
            foreach (var p in mask)
                if (p.x >= x0 && p.x < x1 && p.y >= y0 && p.y < y1) list.Add(p);

            if (list.Count == 0) return mask.First();

            // 코너에 가까운 순 → 상위 30% 중 랜덤
            list.Sort((a, b) =>
            {
                int da = (a.x - x0) * (a.x - x0) + (a.y - y0) * (a.y - y0);
                int db = (b.x - x0) * (b.x - x0) + (b.y - y0) * (b.y - y0);
                return da.CompareTo(db);
            });
            int take = Math.Max(1, list.Count * 3 / 10);
            return list[rng.Next(take)];
        }

        // 지그재그 직결(항상 연결 보장)
        static List<Vector2Int> ConnectDirectZigzag(Vector2Int a, Vector2Int b, int W, int H, GameRandom rng)
        {
            var cur = a;
            var path = new List<Vector2Int> { a };
            int guard = W * H;
            while (cur != b && guard-- > 0)
            {
                var cand = new List<Vector2Int>(2);
                if (b.x != cur.x) cand.Add(new Vector2Int(cur.x + Math.Sign(b.x - cur.x), cur.y));
                if (b.y != cur.y) cand.Add(new Vector2Int(cur.x, cur.y + Math.Sign(b.y - cur.y)));
                cur = cand[rng.Next(cand.Count)]; // 축 교대 지그재그
                path.Add(cur);
            }
            return path;
        }

        // 외길 제약: 인접한 Floor가 정확히 1개일 때만 카빙 허용(루프/두께 방지)
        static bool CanCarve(Vector2Int p, HashSet<Vector2Int> floor, int W, int H)
        {
            if (!InBounds(p, W, H) || floor.Contains(p)) return false;
            int adj = 0;
            foreach (var d in DIR4) if (floor.Contains(p + d)) adj++;
            return adj == 1;
        }

        // 현재 열린 길 가장자리 중 CanCarve 가능한 프런티어 수집
        static List<Vector2Int> CollectFrontierStrict(HashSet<Vector2Int> floor, int W, int H)
        {
            var list = new List<Vector2Int>(128);
            foreach (var f in floor)
                foreach (var d in DIR4)
                {
                    var n = f + d;
                    if (!InBounds(n, W, H) || floor.Contains(n)) continue;
                    if (CanCarve(n, floor, W, H)) list.Add(n);
                }
            return list;
        }

        // seed에서 시작해 len칸 정도 외길 카빙(항상 CanCarve 유지)
        static void CarveSinglePath(Vector2Int seed, int len, HashSet<Vector2Int> floor, int W, int H, GameRandom rng)
        {
            var cur = seed;
            int steps = 0, guard = len * 3;
            while (steps < len && guard-- > 0)
            {
                if (!CanCarve(cur, floor, W, H)) break;
                floor.Add(cur);

                // 다음 후보(아직 벽 & CanCarve 가능)
                var cand = new List<Vector2Int>(4);
                foreach (var d in DIR4)
                {
                    var n = cur + d;
                    if (InBounds(n, W, H) && !floor.Contains(n) && CanCarve(n, floor, W, H))
                        cand.Add(n);
                }
                if (cand.Count == 0) break;

                // 약한 바깥쪽 선호(편중 완화에 도움되되 과하지 않게)
                float Score(Vector2Int n)
                {
                    int cx = W / 2, cy = H / 2;
                    float outward = (Mathf.Abs(n.x - cx) + Mathf.Abs(n.y - cy))
                                  - (Mathf.Abs(cur.x - cx) + Mathf.Abs(cur.y - cy));
                    return 1f + (outward > 0 ? 0.4f : 0f) + rng.NextFloat();
                }
                cand.Sort((a, b) => Score(b).CompareTo(Score(a)));
                cur = cand[0];
                steps++;
            }
        }

        // 심볼 배치(막다른 길 우선)
        static void PlaceSymbols(ExploreMapConfig cfg, GameRandom rng, GeneratedMap r)
        {
            var degree = new Dictionary<Vector2Int, int>(r.Walkable.Count);
            foreach (var p in r.Walkable)
            {
                int d = 0; foreach (var dir in DIR4) if (r.Walkable.Contains(p + dir)) d++;
                degree[p] = d;
            }

            var pool     = r.Walkable.Where(p => p != r.Start && p != r.Boss).ToList();
            var deadEnds = pool.Where(p => degree[p] <= 1).ToList();
            Shuffle(deadEnds, rng); Shuffle(pool, rng);
            int di = 0, pi = 0;

            // 이벤트 후보: config 우선, 없으면 EventDB 키
            List<SystemEnum.CellEventType> evCands = null;
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

            // 덮어쓰기 방지
            r.CellType[r.Start] = SystemEnum.MapCellType.StartPoint;
            r.CellType[r.Boss]  = SystemEnum.MapCellType.BossBattle;
        }

        // 안전망: 연결 안 된 컴포넌트가 있으면 최단 맨해튼으로 바로 연결
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
                    if (!InBounds(n, W, H) || !r.Walkable.Contains(n) || reached.Contains(n)) continue;
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
                    int d = Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
                    if (d < bestD) { best = b; bestD = d; }
                }
                var path = ConnectDirectZigzag(a, best, W, H, rng);
                foreach (var p in path)
                {
                    r.Walkable.Add(p);
                    r.CellType[p] = SystemEnum.MapCellType.Floor;
                    reached.Add(p);
                }
                notReached = r.Walkable.Where(p => !reached.Contains(p)).ToList();
            }
            r.CellType[r.Start] = SystemEnum.MapCellType.StartPoint;
            r.CellType[r.Boss]  = SystemEnum.MapCellType.BossBattle;
        }

        static void Shuffle<T>(IList<T> list, GameRandom rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            { int j = rng.Next(i + 1); (list[i], list[j]) = (list[j], list[i]); }
        }
    }
}
