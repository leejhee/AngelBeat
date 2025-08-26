using UnityEngine;
using novel;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class NovelManager : MonoBehaviour
{
    public static NovelManager Instance { get; private set; }
    public NovelResources Data { get; private set; } = new NovelResources();
    private const string novelPlayerPrefabPath = "NovelPlayer";
    public static NovelPlayer novelPlayer { get; private set; }

    public Task Initialization => _initialization;
    public bool IsReady { get; private set; }

    Task _initialization = Task.CompletedTask;
    bool _initStarted;

    public static async Task<NovelManager> EnsureInitialized()
    {
        // 1) 오브젝트/컴포넌트 확보 (없으면 생성)
        if (Instance == null)
        {
            var go = GameObject.Find("@Novel") ?? new GameObject("@Novel");
            var mgr = go.GetComponent<NovelManager>() ?? go.AddComponent<NovelManager>();
            Instance = mgr;
        }

        // 2) 초기화 1회만 실행
        await Instance.InitializeIfNeededAsync();
        return Instance;
    }
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance ??= this;
        gameObject.name = "@Novel"; // 이름 강제
    }

    public Task InitializeIfNeededAsync()
    {
        if (_initStarted) return _initialization;
        _initStarted = true;
        _initialization = InitializeAsync();
        return _initialization;
    }

    private async Task InitializeAsync()
    {
        // 라벨로 SO 전체 로드
        await Data.InitByLabelAsync();

        if (novelPlayer == null)
        {
            AsyncOperationHandle<GameObject> handle =
                    Addressables.LoadAssetAsync<GameObject>(novelPlayerPrefabPath);
            GameObject prefab = await handle.Task;

            if (prefab != null)
            {
                novelPlayer = Instantiate(prefab, this.transform).GetComponent<NovelPlayer>();
                novelPlayer.gameObject.name = "NovelPlayer";
                DontDestroyOnLoad(novelPlayer);
            }
            else
            {
                Debug.LogError($"[NovelManager] Failed to load prefab: {novelPlayerPrefabPath}");
            }
        }
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

    //private SerializableDict<string, NovelCharacterSO> _characterSODict = new();
    //private const string characterSOPath = "Novel/NovelResourceData/CharacterData/CharacterSO";

    //public SerializableDict<string, NovelCharacterSO> characterSODict
    //{
    //    get { return _characterSODict; }
    //    private set { _characterSODict = value; }
    //}

    //public  void CreateCharacterSOAssets()
    //{
    //    string[] characterNames = Enum.GetNames(typeof(CharacterName));
    //    foreach (var characterName in characterNames)
    //    {
    //        Debug.Log($"{characterName}");
    //        NovelCharacterSOFactory.CreateSpriteDataFromAtlas(characterName);
    //    }
    //}
    //private void LoadCharacterSO()
    //{
    //    _characterSODict.Clear();

    //    string[] characterNames = Enum.GetNames(typeof(CharacterName));
    //    NovelCharacterSO[] characterSOs = ResourceManager.LoadAllAssets<NovelCharacterSO>(characterSOPath);
    //    if (characterSOs == null || characterSOs.Length == 0)
    //    {
    //        Debug.LogError($"캐릭터 SO 불러오기 실패 : {characterSOPath}");
    //    }
    //    else
    //    {
    //        Debug.Log($"{characterNames.Length}명 SO 불러옴");
    //    }

    //    foreach (var character in characterSOs)
    //    {
    //        _characterSODict.Add(character.name, character);
    //    }

    //}
    //public NovelCharacterSO GetCharacterSO(string name)
    //{
    //    NovelCharacterSO characterSO = _characterSODict.GetValue(name);
    //    //_characterSODict.TryGetValue(name, out characterSO);
    //    if (characterSO == null)
    //    {
    //        Debug.LogError($"{name} SO 불러오기 실패");
    //        return null;
    //    }
    //    return characterSO;
    //}
    public void PlayScript(string scriptTitle)
    {

    }


}