using novel;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;

public class NovelManager : MonoBehaviour
{
    public static NovelManager Instance
    {
        get => instance;
        set => instance = value;
    }

    // {
    //     get
    //     {
    //         if (instance == null)
    //         {
    //             Init();
    //         }
    //         return instance;
    //     }
    //     private set =>  instance = value;
    // }
    private static NovelManager instance;
    public static NovelResources Data { get; private set; } = new NovelResources();

    public NovelAudioManager Audio { get; private set; } = new();

    private const string NovelPlayerPrefabPath = "NovelPlayer";
    public static NovelPlayer Player { get; private set; }

    private static readonly SemaphoreSlim NovelPlayerGate = new SemaphoreSlim(1, 1);

    public UniTask Initialization => _initialization;
    private static bool isReady;

    UniTask _initialization = UniTask.CompletedTask;
    static bool initStarted;

    public static async void Init()
    {
        Debug.Log("Init");
        if (instance == null)
        {
            // NovelManager가 이미 씬에 존재하는지 확인
            var existing = FindObjectOfType<NovelManager>();
            // 있으면 그걸 사용, 없으면 새로 생성
            Instance = existing ?? new GameObject("@Novel").AddComponent<NovelManager>();
        }

        // 메인 스레드에서 초기화 해야함
        await Instance.InitializeIfNeededAsync();

    }

    public static async UniTask<NovelManager> InitAsync()
    {
        Debug.Log("Init Async");
        if (instance == null)
        {
            // NovelManager가 이미 씬에 존재하는지 확인
            var existing = FindObjectOfType<NovelManager>();
            // 있으면 그걸 사용, 없으면 새로 생성
            Instance = existing ?? new GameObject("@Novel").AddComponent<NovelManager>();
        }
        await Instance.InitializeIfNeededAsync();

        if (initStarted)
        {
            // 초기화가 시작되었을 경우
            while (!isReady)
            {
                // 완료될때까지 대기
                await UniTask.Yield();
            }
        }
        
        return Instance;
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
    private UniTask InitializeIfNeededAsync()
    {
        // 이미 초기화가 시작되었으면 기존 Task 반환
        if (initStarted)
        {
            Debug.Log("[NovelManager] Initialization already started, returning existing task.");
            return _initialization;
        }


        // 초기화 시작
        initStarted = true;
          _initialization = InitializeAsync();

        return _initialization;
    }

    // 여기가 진짜 초기화 코드
    private async UniTask InitializeAsync()
    {
        // 라벨로 SO 전체 로드
        await Data.InitByLabelAsync();
  
        // 오디오매니저 초기화
        await Audio.AudioManagerInitAsync();

        isReady = true;
        Debug.Log("Novel Engine 초기화 완료");
    }
    public static async UniTask ShutdownAsync()
    {
        if (Instance == null) return;

        try { Data.OnNovelEnd(); } catch { /* ignore */ }

        if (Player != null)
        {
            Addressables.ReleaseInstance(Player.gameObject);
            Player = null;
        }

        var go = Instance.gameObject;
        Instance = null;
        if (go != null) Destroy(go);

        
        await UniTask.Yield();
    }

    public async UniTask PlayScript(string scriptTitle)
    {
        if (instance == null)
        {
            await NovelManager.InitAsync();
        }
        Instance.PlayScriptAsync(scriptTitle);
    }
    private async UniTask PlayScriptAsync(string scriptTitle)
    {

        if (!isReady)
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
        Player.SetScript(script);
        // 플레이 해줌
        Player.Play();

    }
    private async UniTask InstantiateNovelPlayerAsync()
    {
        Debug.Log("이거 떠야함");
        await NovelPlayerGate.WaitAsync();
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


            // 없으면 Addressable에서 로드 및 인스턴스화
            if (Player == null)
            {
                AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(NovelPlayerPrefabPath, transform);
                var go = await handle.Task;

                if (go != null)
                {
                    go.name = "NovelPlayer";
                    Player = go.GetComponent<NovelPlayer>();
                }
                else
                {
                    Debug.LogError($"[NovelManager] Failed to load prefab: {NovelPlayerPrefabPath}");
                }
            }

        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[NovelManager] Error while checking for existing NovelPlayer: {ex.Message}");
            return;
        }
        finally
        {
            NovelPlayerGate.Release();
        }
        Debug.Log(" [NovelManager] NovelPlayer instantiated.");
    }

    public void ReleaseNovelPlayer()
    {
        Addressables.ReleaseInstance(Player.gameObject);
    }
}