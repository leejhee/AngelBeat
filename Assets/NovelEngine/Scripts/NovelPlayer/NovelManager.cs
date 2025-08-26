using novel;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;


public class NovelManager : MonoBehaviour
{
    public static NovelManager Instance { get; private set; }
    public  NovelResources Data { get; private set; } = new NovelResources();


    private const string novelPlayerPrefabPath = "NovelPlayer";
    public static NovelPlayer novelPlayer { get; private set; }

    private static readonly SemaphoreSlim _novelPlayerGate = new SemaphoreSlim(1, 1);

    public Task Initialization => _initialization;
    public bool IsReady { get; private set; }

    Task _initialization = Task.CompletedTask;
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
        _ = Instance.InitializeIfNeededAsync();

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
    public Task InitializeIfNeededAsync()
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
    private async Task InitializeAsync()
    {
        // 라벨로 SO 전체 로드
        await Data.InitByLabelAsync();



        IsReady = true;
    }
    public static async Task ShutdownAsync()
    {
        if (Instance == null) return;
        try { Instance.Data.OnNovelEnd(); } catch { /* ignore */ }

        if (novelPlayer != null)
        {
            Destroy(novelPlayer.gameObject);
            novelPlayer = null;
        }
        var go = Instance.gameObject;
        Instance = null;
        if (go != null) Destroy(go);

        await Task.Yield();
    }
    public async void PlayScript(string scriptTitle)
    {
        if (IsReady == false)
        {
            Debug.LogError("[NovelManager] Not ready yet. Call InitializeAsync() first.");
            return;
        }
        // 노벨 플레이어 인스턴스화 시켜줌
        await InstantiateNovelPlayer();

        // 원하는 스크립트 장착
        TextAsset script = Data.script.GetScriptByTitle(scriptTitle);
        if (script == null)
        {
            Debug.LogError($"[NovelManager] Script '{scriptTitle}' not found.");
            return;
        }
        novelPlayer.novelScript = script;
        // 플레이 해줌
        novelPlayer.Play();

    }
    private async Task InstantiateNovelPlayer()
    {
        await _novelPlayerGate.WaitAsync();
        try
        {
            // 가장 먼저 NovelPlayer 컴포넌트가 자식 오브젝트에 있는지 확인
            if (novelPlayer == null)
                novelPlayer = GetComponentInChildren<NovelPlayer>(true);
            else
            {
                Debug.Log(" [NovelManager] NovelPlayer already exists.");
                return;
            }


            // 없으면 Addressables에서 로드 및 인스턴스화
            if (novelPlayer == null)
            {
                AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(novelPlayerPrefabPath);
                var prefab = await handle.Task;

                if (prefab != null)
                {
                    var go = Instantiate(prefab, transform);
                    go.name = "NovelPlayer";
                    novelPlayer = go.GetComponent<NovelPlayer>();
                    DontDestroyOnLoad(novelPlayer);
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