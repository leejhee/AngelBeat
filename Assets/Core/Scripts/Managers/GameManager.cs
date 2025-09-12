using Core.Scripts.Foundation.Define;
using Core.Scripts.Foundation.Singleton;
using Core.Scripts.Foundation.Utils;
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
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
        
        #region Singleton Objects Management
        [Serializable]
        public class ManagerDictionary : SerializableDict<SystemEnum.ManagerType, ManagerBehaviour> {}
        
        [SerializeField, Header("상태에 따른 관리")]
        private ManagerDictionary managerDict = new();

        
        
        
        #endregion
        
        
        private async void Start()
        {
            try
            {
                var ct = this.GetCancellationTokenOnDestroy();
                await ResourceManager.Instance.InitAsync().AttachExternalCancellation(ct);
                await DataManager.Instance.InitAsync().AttachExternalCancellation(ct);
                
                Init();
            }
            catch (OperationCanceledException) { }
            catch (Exception e)
            {
                Debug.LogException(e);
            } 
           

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

            //산하에 SingletonObject<T> 상속받는 매니저들 동기 초기화. Managers 어셈블리 내의 매니저들만 Init하자.
            //나머지는 전부 런타임에 게임플레이 어딘가(씬 등)에서 호출합니다.
            SaveLoadManager.Instance.Init();
            SoundManager.Instance.Init();
            InputManager.Instance.Init();
        }

        private void Update()
        {
            InputManager.Instance.OnUpdate();
        }

        private void OnApplicationQuit()
        {
            //강종 대비
            SaveLoadManager.Instance.OnApplicationQuit();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            //모바일 사례 : 갑자기 내린다면
            SaveLoadManager.Instance.OnApplicationPause(pauseStatus);
        }
    }
}

