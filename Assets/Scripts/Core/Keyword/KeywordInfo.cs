using AngelBeat.Core.SingletonObjects.Managers;
using System.Collections.Generic;

namespace AngelBeat.Core
{
    public class KeywordInfo
    {
        private Dictionary<SystemEnum.eKeyword, KeywordBase> _keywordDict = new();
        private CharBase _keywordOwner;
        public KeywordInfo(CharBase owner)
        {
            
        }
        
        public void AddKeyword(SystemEnum.eKeyword keyword, KeywordBase keywordBase)
        {
            if(!_keywordDict.ContainsKey(keyword))
                _keywordDict.Add(keyword, keywordBase);
            else
                _keywordDict[keyword].ChangeEffect(keywordBase.EffectValue, keywordBase.EffectCount);
        }

        public bool HasKeyword(SystemEnum.eKeyword keyword)
        {
            return _keywordDict.ContainsKey(keyword);
        }

        public void RemoveKeyword(SystemEnum.eKeyword keyword)
        {
            _keywordDict.Remove(keyword);
        }
        
        public int GetKeywordCount(SystemEnum.eKeyword keyword) => 
            _keywordDict.ContainsKey(keyword) ? _keywordDict[keyword].EffectCount : 0;
        
        public int GetKeywordValue(SystemEnum.eKeyword keyword) => 
            _keywordDict.ContainsKey(keyword) ? _keywordDict[keyword].EffectValue : 0;
        
        public void KeywordChange(OnKeywordChange onKeywordChange)
        {
            
        }
    }    
}
