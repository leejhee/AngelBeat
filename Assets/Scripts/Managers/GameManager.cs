using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    static GameManager _inst;
    public static GameManager Inst
    {
        get
        {
            if (_inst == null)
            {
                _inst = new GameManager();
            }
            return _inst;
        }
    }

    ResourceManager _resource = new ResourceManager();
    UIManager _ui_manager = new UIManager();
    SoundManager _sound = new SoundManager();
    SaveLoadManager _saveLoad = new SaveLoadManager();

    public static ResourceManager Resource { get { return Inst._resource; } }
    public static UIManager UI { get { return Inst._ui_manager; } }
    public static SoundManager Sound { get { return Inst._sound; } }
    public static SaveLoadManager SaveLoad { get { return Inst._saveLoad; } }


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

            DontDestroyOnLoad(go);
            _inst = go.GetComponent<GameManager>();

        }
    }
}
