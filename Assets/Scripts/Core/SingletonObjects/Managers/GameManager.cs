using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.UIElements;

namespace AngelBeat
{
    public class GameManager : MonoBehaviour
    {
        #region 싱글턴.
        static GameManager _instance;
        public static GameManager Instance
        {
            get
            {
                Init(); return _instance ;
            }
        }
        GameManager() { }
        #endregion
    
        #region 없앨 것.
        //[TODO] : 필요 시 전부 SingletonObject<T>로 상속받도록 구조 바꿀것.
        InputManager _input = new InputManager();
        public static InputManager Input { get { return Instance._input; } }
        #endregion
    
        private void Start()
        {
            Init();
        }
        private static void Init()
        {
            if (_instance == null)
            {
                GameObject go = GameObject.Find("@GameManager");
                if (go == null)
                {
                    go = new GameObject { name = "@GameManager" };
                    go.AddComponent<GameManager>();
                }

                _instance = go.GetComponent<GameManager>();
                DontDestroyOnLoad(go);

                //산하에 SingletonObject<T> 상속받는 매니저들 초기화.
                DataManager.Instance.Init();
                StageManager.Instance.Init();
                NovelManager.Instance.Init();
                SoundManager.Instance.Init();
            }
        }

        private void Update()
        {
            _input.OnUpdate();
        }
    }
}

