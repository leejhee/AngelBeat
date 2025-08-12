using System;
using UnityEngine;

namespace Core.SingletonObjects.Managers
{
    public class GameManager : MonoBehaviour
    {
        #region 싱글턴.

        private static GameManager instance;
        public static GameManager Instance
        {
            get
            {
                if(!instance) Init(); 
                return instance ;
            }
        }
        GameManager() { }
        #endregion
        
        #region Game State Management
        private SystemEnum.GameState _state;
        public SystemEnum.GameState GameState
        {
            get => _state;
            set
            {
                BeforeGameStateChange?.Invoke(_state);
                _state = value;
                OnGameStateChanged?.Invoke(value);
            }
        }
        
        public event Action<SystemEnum.GameState> BeforeGameStateChange;
        public event Action<SystemEnum.GameState> OnGameStateChanged;
        #endregion
        
        
        
        private void Start()
        {
            Init();
        }
        private static void Init()
        {
            GameObject go = GameObject.Find("@GameManager");
            if (!go)
            {
                go = new GameObject { name = "@GameManager" };
                go.AddComponent<GameManager>();
            }

            instance = go.GetComponent<GameManager>();
            DontDestroyOnLoad(go);

            //산하에 SingletonObject<T> 상속받는 매니저들 초기화.
            DataManager.Instance.Init();
            SaveLoadManager.Instance.Init();
            NovelManager.Instance.Init();
            SoundManager.Instance.Init();
            InputManager.Instance.Init();
            
            
        }

        private void Update()
        {
            InputManager.Instance.OnUpdate();
        }
    }
}

