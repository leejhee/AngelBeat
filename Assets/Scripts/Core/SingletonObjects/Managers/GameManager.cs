using Modules.RoguelikeNodeMap;
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
                InputManager.Instance.Init();
            }
        }

        private void Update()
        {
            InputManager.Instance.OnUpdate();
        }
    }
}

