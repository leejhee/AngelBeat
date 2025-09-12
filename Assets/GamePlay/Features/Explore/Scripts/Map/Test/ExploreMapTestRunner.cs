using System.Collections.Generic;
using UnityEngine;
using Core.Scripts.Foundation.Define;
using Core.Scripts.Managers;
using GamePlay.Features.Explore.Scripts.Map.Data;
using GamePlay.Features.Explore.Scripts.Map.Logic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class ExploreMapTestRunner : MonoBehaviour
{
    [Header("Grid (Required)")]
    [SerializeField] private GridLayout grid;
    [SerializeField] private Transform mapRoot;

    [Header("DBs")]
    [SerializeField] private ExploreMapConfigDB configDB;
    [SerializeField] private ExploreSymbolDB   symbolDB; // Start/Boss/Battle/Item
    [SerializeField] private ExploreEventDB    eventDB;  // Event 소분류

    [Header("Params")]
    [SerializeField] private SystemEnum.Dungeon dungeon = SystemEnum.Dungeon.MOUNTAIN_BACK;
    [SerializeField] private int floor = 1;
    [SerializeField] private int seed = 12345;
    [SerializeField] private bool autoRollSeedOnGenerate = true; // Generate 때 자동 난수 시드
    [SerializeField] private bool useSlotSeedWhenPlaying = false;

    [Header("Visual")]
    [SerializeField] private bool spawnFloorUnderSymbols = true;
    [SerializeField] private bool setSortingOrder = true;
    [SerializeField] private int sortingBase = 100000;
    [SerializeField] private int sortingStep = 10;

    [ContextMenu("Explore/Generate (Grid2D Rect)")]
    public void Generate()
    {
        if (!grid)     { Debug.LogError("[Runner] GridLayout 미할당"); return; }
        if (!configDB) { Debug.LogError("[Runner] ConfigDB 미할당"); return; }
        if (!mapRoot)
        {
            var go = GameObject.Find("ExploreMapRoot") ?? new GameObject("ExploreMapRoot");
            mapRoot = go.transform;
        }
        Clear();

        // Seed 결정(항상 양수로 굴림)
        if (autoRollSeedOnGenerate)
        {
            seed = UnityEngine.Random.Range(0, int.MaxValue); // 0~2,147,483,647
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            if (!Application.isPlaying) EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif
        }
        int usedSeed = seed >= 0 ? seed : -seed; // 혹시 수동으로 음수를 넣어도 안전

        if (Application.isPlaying && useSlotSeedWhenPlaying)
        {
            try { usedSeed = (int)(SaveLoadManager.Instance.CurrentSlot.slotSeed & 0x7fffffff); }
            catch { /* 옵션 */ }
        }

        // 주입 & 생성
        ExploreMapGenerator.SetConfigDB(configDB);
        ExploreMapGenerator.SetEventDB(eventDB);
        var r   = ExploreMapGenerator.GenerateRect(dungeon, floor, usedSeed);
        var cfg = configDB.GetConfig(dungeon, floor);
        if (r == null || !cfg) { Debug.LogError("[Runner] 생성 실패/Config 없음"); return; }

        // 심볼 DB 접근(대분류)
        bool TryGetSymbol(SystemEnum.MapCellType t, out GameObject pf)
        { pf = null; return symbolDB != null && symbolDB.TryGet(t, out pf) && pf; }

        // 이벤트 프리팹(소분류)
        var eventPrefab = new Dictionary<SystemEnum.CellEventType, GameObject>();
        if (eventDB != null && eventDB.symbols != null)
            foreach (var s in eventDB.symbols) eventPrefab[s.eventType] = s.symbolPrefab;

        // 배치
        foreach (var kv in r.CellType)
        {
            var pos = kv.Key;
            var tp  = kv.Value;
            var w   = CellToWorld(pos);

            if (tp == SystemEnum.MapCellType.Wall) { Inst(cfg.boundPrefab, w, pos.y); continue; }
            if (spawnFloorUnderSymbols)             Inst(cfg.floorPrefab, w, pos.y);

            if (tp == SystemEnum.MapCellType.StartPoint && TryGetSymbol(SystemEnum.MapCellType.StartPoint, out var sp))
                Inst(sp, w + new Vector3(0, 0, -1f), pos.y); // ★ StartPoint만 z = -1
            else if (tp == SystemEnum.MapCellType.BossBattle && TryGetSymbol(SystemEnum.MapCellType.BossBattle, out var bp))
                Inst(bp, w, pos.y);
            else if (tp == SystemEnum.MapCellType.Battle && TryGetSymbol(SystemEnum.MapCellType.Battle, out var bat))
                Inst(bat, w, pos.y);
            else if (tp == SystemEnum.MapCellType.Item && TryGetSymbol(SystemEnum.MapCellType.Item, out var it))
                Inst(it, w, pos.y);
            else if (tp == SystemEnum.MapCellType.Event &&
                     r.EventAt.TryGetValue(pos, out var ev) &&
                     eventPrefab.TryGetValue(ev, out var ep) && ep)
                Inst(ep, w, pos.y);
        }
    }

    [ContextMenu("Explore/Clear")]
    public void Clear()
    {
        if (!mapRoot) return;
#if UNITY_EDITOR
        while (mapRoot.childCount > 0) DestroyImmediate(mapRoot.GetChild(0).gameObject);
#else
        foreach (Transform c in mapRoot) Destroy(c.gameObject);
#endif
    }

    // 시드만 굴리고 싶을 때(양수)
    [ContextMenu("Explore/Roll Seed Only")]
    public void RollSeedOnly()
    {
        seed = UnityEngine.Random.Range(0, int.MaxValue);
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        if (!Application.isPlaying) EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif
        Debug.Log($"[Runner] Rolled Seed: {seed}");
    }

    Vector3 CellToWorld(Vector2Int c) => grid.CellToWorld(new Vector3Int(c.x, c.y, 0));

    void Inst(GameObject prefab, Vector3 pos, int cellY)
    {
        if (!prefab) return;
#if UNITY_EDITOR
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, mapRoot);
        go.transform.position = pos;
#else
        var go = Instantiate(prefab, pos, Quaternion.identity, mapRoot);
#endif
        if (setSortingOrder && prefab)
        {
            var sr = (prefab ? prefab.GetComponentInChildren<SpriteRenderer>() : null);
            sr = sr ?? (mapRoot.GetChild(mapRoot.childCount - 1)?.GetComponentInChildren<SpriteRenderer>());
            if (sr) sr.sortingOrder = sortingBase - (cellY * sortingStep);
        }
    }
}
