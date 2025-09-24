using Core.Scripts.Foundation.Define;
using Core.Scripts.Foundation.Utils;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UIs.Runtime
{
    /// <summary>
    /// GameManager와 같은, DDOL Manager
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        #region Singleton
        private static UIManager instance;

        public static UIManager Instance
        {
            get
            {
                if (!instance) Init();
                return instance;
            }
        }
        
        private static void Init()
        {
            GameObject go = GameObject.Find("@UIManager");
            if (!go)
            {
                go = new GameObject { name = "@UIManager" };
                go.AddComponent<UIManager>();
            }

            instance = go.GetComponent<UIManager>();
            DontDestroyOnLoad(go);
        }
        
        #endregion
        
        /// <summary>
        /// View에 대한 데이터베이스
        /// </summary>
        [SerializeField] private SerializableDict<SystemEnum.GameState, ViewCatalog> catalogDict;

        private ViewCatalog _focusingCatalog;
        
        
        /// <summary>
        /// Presenter을 관리하는 스택
        /// </summary>
        private static Stack<IPresenter> presenterStack = new();

        
        /// <summary>
        /// 모든 UI의 Root 위치
        /// </summary>
        private Transform UIRoot
        {
            get
            {
                GameObject go = GameObject.Find("@UIRoot");
                if (!go) 
                {
                    go = new GameObject { name = "@UIRoot" };
                }
                return go.transform;
            }
        }
        
        public async UniTask 어싱크로띄우기(ViewID viewID)
        {
            
        }
        
        
    }
}