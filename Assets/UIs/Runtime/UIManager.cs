using Core.Scripts.Foundation.Define;
using Core.Scripts.Foundation.Utils;
using Core.Scripts.Managers;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

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
        
        #region DB
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
        
        private void ChangeCatalog(SystemEnum.GameState state) => _focusingCatalog = _catalogDict.GetValueOrDefault(state);
        
        #endregion
        
        #region Stack & Cache
        private struct OpenedUI
        {
            public ViewID ID; 
            public IPresenter Presenter;
            public GameObject Go;
        }
        
        private readonly Stack<OpenedUI> _stack = new();
        
        
        /// <summary>
        /// View의 캐싱을 위한 딕셔너리.
        /// </summary>
        private Dictionary<ViewID, Stack<GameObject>> _cache = new();

        [SerializeField] private int cacheCapacity = 5;

        private async UniTask<GameObject> GetViewFromCache(ViewID id, ViewCatalog.ViewEntry entry, Transform parent)
        {
            if (_cache.TryGetValue(id, out Stack<GameObject> st) && st.Count > 0)
            {
                GameObject go = st.Pop();
                go.transform.SetParent(parent, false);
                go.SetActive(true);
                go.transform.SetAsLastSibling();
                return go;
            }

            GameObject inst =
                await ResourceManager.Instance.InstantiateAsync(entry.viewReference, parent, false, _uiCts.Token);
            inst.transform.SetAsLastSibling();
            return inst;
        }

        private void ReturnViewToCache(ViewID id, GameObject go)
        {
            if (!go) return;
            go.SetActive(false);
            go.transform.SetParent(_cacheRoot ? _cacheRoot : _uiRoot, false);

            if (!_cache.TryGetValue(id, out Stack<GameObject> st))
            {
                _cache[id] = st = new Stack<GameObject>();
            }

            if (st.Count < cacheCapacity) st.Push(go);
            else ResourceManager.Instance.ReleaseInstance(go);
        }
        
        #endregion
        
        #region Root & Layer Transform
        [SerializeField] private AssetReferenceGameObject rootPrefabReference;

        private Transform _uiRoot;
        private Transform _mainRoot, _modalRoot, _systemRoot;
        private Transform _cacheRoot;
        
        private async UniTask EnsureRoot(CancellationToken token)
        {
            if (_uiRoot) return;

            var go = await ResourceManager.Instance.InstantiateAsync(rootPrefabReference, null, false, token);
            _uiRoot = go.transform;
            
            _mainRoot   = _uiRoot.Find("@MainRoot")   ?? CreateLayer("@MainRoot",   0);
            _modalRoot  = _uiRoot.Find("@ModalRoot")  ?? CreateLayer("@ModalRoot",  100);
            _systemRoot = _uiRoot.Find("@SystemRoot") ?? CreateLayer("@SystemRoot", 1000);
            _cacheRoot  = _uiRoot.Find("@UICache")    ?? CreateCache("@UICache");
        }

        private Transform CreateLayer(string layerName, int order)
        {
            GameObject g = new(layerName);
            g.transform.SetParent(_uiRoot, false);
            Canvas c = g.AddComponent<Canvas>();
            c.overrideSorting = true; c.sortingOrder = order;
            g.AddComponent<GraphicRaycaster>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            return g.transform;
        }

        private Transform CreateCache(string cacheName)
        {
            GameObject g = new(cacheName);
            g.transform.SetParent(_uiRoot, false);
            return g.transform;
        }
        
        #endregion
        
        /// <summary>
        /// UI 전용 CTS
        /// </summary>
        private CancellationTokenSource _uiCts;
        
        private void Awake()
        {
            if (instance != null && instance != this) { Destroy(gameObject); return; }
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            GameManager.Instance.OnGameStateChanged += ChangeCatalog;
            foreach (CatalogEntry pair in entries)
            {
                _catalogDict[pair.keyState] = pair.catalog; //빠른 인덱싱을 위해
            }
            _focusingCatalog = _catalogDict[SystemEnum.GameState.Lobby]; // 초기 포커싱 설정
            _uiCts = new CancellationTokenSource();
        }
        
        public async UniTask ShowViewAsync(ViewID viewID)
        {
            
            if (!_focusingCatalog) return;

            await EnsureRoot(_uiCts.Token);

            if (!_focusingCatalog.TryGet(viewID, out ViewCatalog.ViewEntry viewRef))
            {
                Debug.LogWarning($"[UIManager] No Entry for {viewID}");
                return;
            }

            GameObject go = null;
            try
            {
                Transform parent = _mainRoot;
                go = await GetViewFromCache(viewID, viewRef, parent);
                if (!go.TryGetComponent(out IView view))
                {
                    ResourceManager.Instance.ReleaseInstance(go);
                    Debug.LogError($"[UIManager] No IView in Prefab for {viewID}");
                    return;
                }

                IPresenter presenter = _focusingCatalog.GetPresenter(viewID, view);
                _stack.Push(new OpenedUI { ID = viewID, Presenter = presenter, Go = go });

                using var linked = CancellationTokenSource.CreateLinkedTokenSource(_uiCts.Token);
                await presenter.OnEnterAsync(linked.Token);

                go = null;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                if (go) ResourceManager.Instance.ReleaseInstance(go);
                throw;
            }
        }

        

        public async UniTask HideTopViewAsync()
        {
            if (_stack.Count == 0) return;
            OpenedUI current = _stack.Pop();
            
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(_uiCts.Token);
            try
            {
                await current.Presenter.OnExitAsync(linked.Token);
            }
            finally
            {
                current.Presenter.Dispose();
                ReturnViewToCache(current.ID, current.Go);
            }
        }
        
        /// <summary>
        /// 게임이 종료될 때긴 하지만, 만일을 위해 묶인 모든 리소스와 이벤트를 해제한다.
        /// </summary>
        private void OnDestroy()
        {
            GameManager.Instance.OnGameStateChanged -= ChangeCatalog;

            while (_stack.Count > 0)
            {
                OpenedUI top = _stack.Pop();
                try { top.Presenter?.Dispose(); }
                catch { }
                
                if(top.Go) ResourceManager.Instance.ReleaseInstance(top.Go);
            }

            foreach (var kv in _cache)
            {
                Stack<GameObject> st = kv.Value;
                while (st.Count > 0)
                {
                    GameObject go = st.Pop();
                    if(go) ResourceManager.Instance.ReleaseInstance(go);
                }
            }
            _cache.Clear();

            if (_uiRoot)
            {
                ResourceManager.Instance.ReleaseInstance(_uiRoot.gameObject);
                _uiRoot = null;
            }
            
            _uiCts?.Cancel();
            _uiCts?.Dispose();
        }
    }
}