using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UIs.Runtime
{
    /// <summary>
    /// 생성할 수 있는 UI 프리팹들의 풀을 key - value로 표현한 형태
    /// </summary>
    [CreateAssetMenu(menuName = "ScriptableObject/UIViewList")]
    public class UIViewList : ScriptableObject
    {
        [Serializable]
        public struct ViewEntry
        {
            public UIView view;
            public UILayer layer;
            public string key;
            public AssetReferenceGameObject viewReference;
        }
        
        [SerializeField]
        private List<ViewEntry> viewTable = new();
        
        public IReadOnlyList<ViewEntry> ViewTable => viewTable;
    }
}
