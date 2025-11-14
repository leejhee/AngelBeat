using Core.Scripts.Data;
using Core.Scripts.Foundation.Define;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using static Core.Scripts.Foundation.Define.SystemEnum;

namespace Core.Scripts.Managers
{
    public partial class DataManager
    {
        private Dictionary<eKeyword, KeywordData> _keywordMap = new();
        public Dictionary<eKeyword, KeywordData> KeywordMap => _keywordMap;
        
        
        /// <summary>
        /// 캐릭터별 스킬 데이터 맵
        /// </summary>
        private Dictionary<long, List<SkillData>> characterSkillMap = new();
        public Dictionary<long, List<SkillData>> CharacterSkillMap => characterSkillMap;
        
        
        private void ClearJoinedMaps()
        {
            //=======Clear Data - Poco Maps==========//
            
            _keywordMap.Clear();
            characterSkillMap.Clear();
            
            //=========================================//
            
        }


        private void SetKeywordDataMap()
        {
            string key = nameof(KeywordData);
            if (_cache.ContainsKey(key) == false)
                return;
            Dictionary<long, SheetData> keywordDict = _cache[key];
            if (keywordDict == null)
            {
                Debug.LogError($"Map not included in parsing : {key}");
                return;
            }

            foreach (var _keyword in keywordDict.Values)
            {
                if (_keyword is not KeywordData keyword) continue;
                if(!_keywordMap.ContainsKey(keyword.keywordType))
                    _keywordMap.Add(keyword.keywordType, keyword);
            }
        }

        
        private void SetCharacterSkillMap()
        {
            const string key = nameof(SkillData);
            if (!_cache.TryGetValue(key, out Dictionary<long, SheetData> skillDict))
                return;

            if (skillDict == null)
            {
                Debug.LogError($"Map not included in parsing : {key}");
                return;
            }
            
            foreach (SheetData skillData in skillDict.Values)
            {
                if (skillData is not SkillData skill) continue;
                if (!characterSkillMap.ContainsKey(skill.characterID))
                {
                    CharacterSkillMap.Add(skill.characterID, new List<SkillData>{ skill });
                }
                else
                {
                    CharacterSkillMap[skill.characterID].Add(skill);
                }
            }
        }
        
        public List<SkillData> GetDefaultSkillSet(long characterID)
        {
            if (characterID <= 0)
            {
                Debug.LogError($"[DataManager] Invalid CharacterID {characterID} : Must be > 0");
                return new List<SkillData>();
            }

            if (!characterSkillMap.TryGetValue(characterID, out List<SkillData> skills))
            {
                Debug.LogError($"[DataManager] Invalid CharacterID : No {characterID} in Data List");
                return new List<SkillData>();
            }

            List<SkillData> defaultSkills = skills.FindAll(x => x.unlockCondition == eSkillUnlock.Default);
            return defaultSkills;
        }
        
    }

}
