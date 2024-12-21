using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    #region 싱글턴.
    static GameManager _inst;
    public static GameManager Inst
    {
        get
        {
            Init(); return _inst ;
        }
    }
    GameManager() { }
    #endregion

    //[TODO] : 필요 시 전부 SingletonObject<T>로 상속받도록 구조 바꿀것.
    InputManager _input = new InputManager();
    ResourceManager _resource = new ResourceManager();
    UIManager _ui_manager = new UIManager();
    SoundManager _sound = new SoundManager();
    //SaveLoadManager _saveLoad = new SaveLoadManager();

    public static InputManager Input { get { return Inst._input; } }
    public static ResourceManager Resource { get { return Inst._resource; } }
    public static UIManager UI { get { return Inst._ui_manager; } }
    public static SoundManager Sound { get { return Inst._sound; } }
    //public static SaveLoadManager SaveLoad { get { return Inst._saveLoad; } }

    private void Awake()
    {
        Init();
    }
    static void Init()
    {
        if (_inst == null)
        {
            GameObject go = GameObject.Find("@GameManager");
            if (go == null)
            {
                go = new GameObject { name = "@GameManager" };
                go.AddComponent<GameManager>();
            }

            _inst = go.GetComponent<GameManager>();
            DontDestroyOnLoad(go);

            //산하에 SingletonObject<T> 상속받는 매니저들 초기화.
            DataManager.Instance.Init();
            StageManager.Instance.Init();
        }
    }

    private void Update()
    {
        _input.OnUpdate();
    }
}
