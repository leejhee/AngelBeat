using Core.Scripts.Foundation.Define;
using Core.Scripts.Foundation.Utils;
using Core.Scripts.Managers;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
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

        public static UIManager Instance => instance;
        
        #endregion
        
        #region Temporary Entry
        
        /// <summary>
        /// SerializableDict의 Inspector상 오류 수정 전까지 사용
        /// </summary>
        [Serializable]
        private class CatalogEntry
        {
            public SystemEnum.GameState keyState;
            public ViewCatalog catalog;
        }
        
        #endregion
        
        /// <summary>
        /// View에 대한 데이터베이스
        /// </summary>
        [SerializeField] private List<CatalogEntry> entries = new();
        
        private ViewCatalog _focusingCatalog;
        
        private readonly Dictionary<SystemEnum.GameState, ViewCatalog> _catalogDict = new();
        
        
        /// <summary>
        /// Presenter을 관리하는 스택
        /// </summary>
        private Stack<IPresenter> presenterStack = new();
        
        
        /// <summary>
        /// UI 전용 CTS
        /// </summary>
        private CancellationTokenSource uiCts = new();
        
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
        
        private void ChangeCatalog(SystemEnum.GameState state) => _focusingCatalog = _catalogDict.GetValueOrDefault(state);
        
        private void Awake()
        {
            // 프리팹화로 인해 여기서 초기화
            if (instance != null && instance != this) { Destroy(gameObject); return; }
            instance = this;
            DontDestroyOnLoad(this);

            GameManager.Instance.OnGameStateChanged += ChangeCatalog;
            foreach (CatalogEntry pair in entries)
            {
                _catalogDict[pair.keyState] = pair.catalog; //빠른 인덱싱을 위해
            }
        }

        public async UniTask ShowViewAsync(ViewID viewID)
        {
            if (!_focusingCatalog) return;
            if (_focusingCatalog.TryGet(viewID, out ViewCatalog.ViewEntry viewRef))
            {
                GameObject go = await ResourceManager.Instance.InstantiateAsync(viewRef.viewReference, UIRoot);
                if (go.TryGetComponent(out IView view))
                {
                    var presenter = _focusingCatalog.GetPresenter(viewID, view);
                    presenterStack.Push(presenter);
                    await presenter.OnEnterAsync(uiCts.Token);
                    await view.PlayEnterAsync(uiCts.Token);
                }
            }
        }

        public async UniTask HideTopViewAsync()
        {
            if (presenterStack.Count == 0) return;
            IPresenter current = presenterStack.Peek();
            await current.OnExitAsync(uiCts.Token);
            presenterStack.Pop();
        }

        private void OnDestroy()
        {
            GameManager.Instance.OnGameStateChanged -= ChangeCatalog;
            uiCts.Cancel();
            uiCts.Dispose();
        }
    }
}