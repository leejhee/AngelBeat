using novel;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;

public class NovelManager : MonoBehaviour
{
    public static NovelManager Instance { get; private set; }
    public static NovelResources Data { get; private set; } = new NovelResources();


    private const string novelPlayerPrefabPath = "NovelPlayer";
    public static NovelPlayer Player { get; private set; }

    private static readonly SemaphoreSlim _novelPlayerGate = new SemaphoreSlim(1, 1);

    public UniTask Initialization => _initialization;
    public static bool IsReady { get; private set; }

    UniTask _initialization = UniTask.CompletedTask;
    bool _initStarted;

    public static void Init()
    {
        if (Instance == null)
        {
            // NovelManager가 이미 씬에 존재하는지 확인
            var existing = FindObjectOfType<NovelManager>();
            // 있으면 그걸 사용, 없으면 새로 생성
            Instance = existing ?? new GameObject("@Novel").AddComponent<NovelManager>();
        }

        // 메인 스레드에서 초기화 해야함
        Instance.InitializeIfNeededAsync().Forget();

    }
    void Awake()
    {
        // 싱글톤 패턴
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance ??= this;

        // 게임 오브젝트 이름 강제
        gameObject.name = "@Novel";

        // 씬 전환시 파괴되지 않도록 설정
        DontDestroyOnLoad(gameObject);
    }
    public UniTask InitializeIfNeededAsync()
    {
        // 이미 초기화가 시작되었으면 기존 Task 반환
        if (_initStarted)
        {
            Debug.Log("[NovelManager] Initialization already started, returning existing task.");
            return _initialization;
        }


        // 초기화 시작
        _initStarted = true;
        _initialization = InitializeAsync();

        return _initialization;
    }

    // 여기가 진짜 초기화 코드
    private async UniTask InitializeAsync()
    {
        // 라벨로 SO 전체 로드
        await Data.InitByLabelAsync();

        //await InstantiateNovelPlayerAsync();


        IsReady = true;
    }
    public static async UniTask ShutdownAsync()
    {
        if (Instance == null) return;
        try { Data.OnNovelEnd(); } catch { /* ignore */ }

        if (Player != null)
        {
            Destroy(Player.gameObject);
            Player = null;
        }
        var go = Instance.gameObject;
        Instance = null;
        if (go != null) Destroy(go);


        // 이거 뭔지 공부할것
        await UniTask.Yield();
    }
    public async void PlayScript(string scriptTitle)
    {
        if (IsReady == false)
        {
            Debug.LogError("[NovelManager] Not ready yet. Call InitializeAsync() first.");
            return;
        }
        // 노벨 플레이어 인스턴스화 시켜줌
        if (Player == null)
            await InstantiateNovelPlayerAsync();

        // 원하는 스크립트 장착
        TextAsset script = Data.script.GetScriptByTitle(scriptTitle);

        if (script == null)
        {
            Debug.LogError($"[NovelManager] Script '{scriptTitle}' not found.");
            return;
        }
        Player.novelScript = script;
        // 플레이 해줌
        Player.Play();

    }
    private async UniTask InstantiateNovelPlayerAsync()
    {

        await _novelPlayerGate.WaitAsync();
        try
        {
            // 가장 먼저 NovelPlayer 컴포넌트가 자식 오브젝트에 있는지 확인
            if (Player == null)
                Player = GetComponentInChildren<NovelPlayer>(true);
            else
            {
                Debug.Log(" [NovelManager] NovelPlayer already exists.");
                return;
            }


            // 없으면 Addressables에서 로드 및 인스턴스화
            if (Player == null)
            {
                AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(novelPlayerPrefabPath);
                var prefab = await handle.Task;

                if (prefab != null)
                {
                    var go = Instantiate(prefab, transform);
                    go.name = "NovelPlayer";
                    Player = go.GetComponent<NovelPlayer>();
                    DontDestroyOnLoad(Player);
                }
                else
                {
                    Debug.LogError($"[NovelManager] Failed to load prefab: {novelPlayerPrefabPath}");
                }
                // prefab 레퍼런스 핸들 해제
                Addressables.Release(handle);

            }

        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[NovelManager] Error while checking for existing NovelPlayer: {ex.Message}");
            return;
        }
        finally
        {
            _novelPlayerGate.Release();
        }
        Debug.Log(" [NovelManager] NovelPlayer instantiated.");
    }

}