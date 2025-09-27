using Core.Scripts.Foundation.Define;
using System;
using UnityEngine;

namespace Core.Scripts.Managers
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
                return instance;
            }
        }
        GameManager() { }
        #endregion
        
        #region Game State Management
        
        // 필요하다! - 저장 구조를 위해서 필요함.
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
        }
        
        #region Events responsible with GameManager
        
        public event Action<QuitParam> OnQuit;
        public event Action<PauseParam> OnPause;
        public event Action OnUpdate;
        
        #endregion
        
        private void Update()
        {
            OnUpdate?.Invoke();
            //InputManager.Instance.OnUpdate();
        }

        private void OnApplicationQuit()
        {
            OnQuit?.Invoke(new QuitParam());
            //강종 대비
            SaveLoadManager.Instance.OnApplicationQuit();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            OnPause?.Invoke(new PauseParam());
            //모바일 사례 : 갑자기 내린다면
            SaveLoadManager.Instance.OnApplicationPause(pauseStatus);
        }
    }
    
    public class QuitParam {}
    public class PauseParam{}
}

