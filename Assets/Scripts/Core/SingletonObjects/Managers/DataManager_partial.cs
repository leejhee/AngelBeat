using AngelBeat;
using System.Collections.Generic;
using UnityEngine;
using static SystemEnum;

namespace Core.SingletonObjects.Managers
{
    public partial class DataManager
    {
        private Dictionary<eKeyword, KeywordData> _keywordMap = new();
        public Dictionary<eKeyword, KeywordData> KeywordMap => _keywordMap;
        public void SetKeywordDataMap()
        {
            string key = typeof(KeywordData).Name;
            if (_cache.ContainsKey(key) == false)
                return;
            Dictionary<long, SheetData> keywordDict = _cache[key];
            if (keywordDict == null)
            {
                Debug.LogError("Map not included in parsing");
                return;
            }

            foreach (var _keyword in keywordDict.Values)
            {
                KeywordData keyword = _keyword as KeywordData;
                _keywordMap.Add(keyword.keywordType, keyword);
            }
        }
    }

}
