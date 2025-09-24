using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UIs.Runtime
{
    /// <summary>
    /// 생성할 수 있는 UI 프리팹들의 풀을 key - value로 표현한 형태
    /// </summary>
    [CreateAssetMenu(menuName = "ScriptableObject/ViewCatalog")]
    public class ViewCatalog : ScriptableObject, ISerializationCallbackReceiver
    {
        [Serializable]
        public struct ViewEntry
        {
            public ViewID viewID;
            public AssetReferenceGameObject viewReference;
        }
        
        [SerializeField] private List<ViewEntry> viewTable = new();

        [SerializeField] private PresenterFactory presenterFactory;
        
        public IReadOnlyList<ViewEntry> ViewTable => viewTable;

        private Dictionary<ViewID, ViewEntry> _map;

        public bool TryGet(ViewID id, out ViewEntry entry)
        {
            _map ??= BuildMap();
            return _map.TryGetValue(id, out entry);
        }

        private Dictionary<ViewID, ViewEntry> BuildMap()
        {
            var d = new Dictionary<ViewID, ViewEntry>(viewTable.Count);
            foreach (var e in viewTable) d[e.viewID] = e;
            return d;
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize() => _map = null;
        void ISerializationCallbackReceiver.OnBeforeSerialize() { }

#if UNITY_EDITOR
        // 에디터 전용 디버깅용도이므로 외부 사용으로 인한 빌드 오류 주의.
        private void OnValidate()
        {
            var seen = new HashSet<ViewID>();
            foreach (var e in viewTable)
            {
                if (e.viewReference.RuntimeKeyIsValid() == false)
                    Debug.LogWarning($"[Catalog] '{name}' has null Addressable for {e.viewID}", this);
                if (!seen.Add(e.viewID))
                    Debug.LogWarning($"[Catalog] '{name}' duplicated ViewID: {e.viewID}", this);
            }
        }
#endif
        
    }
}
