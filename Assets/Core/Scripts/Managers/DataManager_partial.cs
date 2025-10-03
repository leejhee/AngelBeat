using Core.Scripts.Data;
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
        /// 캐릭터 아이콘 스프라이트 맵
        /// </summary>
        private Dictionary<string, Sprite> characterIconSpriteMap = new();
        public Dictionary<string, Sprite> CharacterIconSpriteMap => characterIconSpriteMap;
        
        /// <summary>
        /// 캐릭터 LD 스프라이트 맵
        /// </summary>
        private Dictionary<string, Sprite> characterLDSpriteMap = new();
        public Dictionary<string, Sprite> CharacterLDSpriteMap => characterLDSpriteMap;

        /// <summary>
        /// 스킬 아이콘 스프라이트 맵
        /// </summary>
        private Dictionary<string, Sprite> skillIconSpriteMap = new();
        public Dictionary<string, Sprite> SkillIconSpriteMap =>  skillIconSpriteMap;

        
        /// <summary>
        /// 캐릭터별 스킬 데이터 맵
        /// </summary>
        private Dictionary<long, List<SkillData>> characterSkillMap = new();
        public Dictionary<long, List<SkillData>> CharacterSkillMap => characterSkillMap;
        
        
        /// <summary>
        /// 스킬별 대미지 데이터 맵
        /// </summary>
        private Dictionary<long, SkillRangeData> skillRangeMap = new();
        public Dictionary<long, SkillRangeData> SkillRangeMap => skillRangeMap;
        
        
        public void SetKeywordDataMap()
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

        public async UniTask InitCharacterSpriteMapWithYeon()
        {
            string key = nameof(DokkaebiData);
            if (_cache.ContainsKey(key) == false)
                return;
            Dictionary<long, SheetData> dokkaebi = _cache[key];
            if (dokkaebi == null)
            {
                Debug.LogError($"Map not included in parsing : {key}");
                return;
            }

            foreach (var _dokkaebi  in dokkaebi.Values)
            {
                if (_dokkaebi is not DokkaebiData dok) continue;
                Sprite iconTarget = await ResourceManager.Instance.LoadAsync<Sprite>(dok.SpriteIconRoute);
                Sprite ldTarget = await ResourceManager.Instance.LoadAsync<Sprite>(dok.SpriteLDRoute);
            }
        }
        
        public async UniTask SetCharacterSpriteMap()
        {
            string key = nameof(CompanionData);
            if (_cache.ContainsKey(key) == false)
                return;
            Dictionary<long, SheetData> companionDict = _cache[key];
            if (companionDict == null)
            {
                Debug.LogError($"Map not included in parsing : {key}");
                return;
            }
            
            foreach (var _companion in companionDict.Values)
            {
                if (_companion is not CompanionData companion) continue;
                Sprite iconTarget = await ResourceManager.Instance.LoadAsync<Sprite>(companion.charImage);
                Sprite ldTarget = await ResourceManager.Instance.LoadAsync<Sprite>(companion.charLDRoute);
                characterIconSpriteMap.TryAdd(companion.charImage, iconTarget);
                characterLDSpriteMap.TryAdd(companion.charLDRoute, ldTarget);
            }
        }
        
        public async UniTask SetDokkaebiSkillIconSpriteMap()
        {
            string key = nameof(DokkaebiSkillData);
            if (_cache.ContainsKey(key) == false)
                return;
            Dictionary<long, SheetData> skillDict = _cache[key];
            if (skillDict == null)
            {
                Debug.LogError($"Map not included in parsing : {key}");
                return;
            }
            
            foreach (SheetData _skillData in skillDict.Values)
            {
                if (_skillData is not DokkaebiSkillData doskill) continue;
                Sprite iconTarget = await ResourceManager.Instance.LoadAsync<Sprite>(doskill.skillIconImage);
                skillIconSpriteMap.TryAdd(doskill.skillIconImage, iconTarget);
            }
        }
        
        public async UniTask SetNormalSkillIconSpriteMap()
        {
            string key = nameof(SkillData);
            if (_cache.ContainsKey(key) == false)
                return;
            Dictionary<long, SheetData> skillDict = _cache[key];
            if (skillDict == null)
            {
                Debug.LogError($"Map not included in parsing : {key}");
                return;
            }
            
            foreach (SheetData _skillData in skillDict.Values)
            {
                if (_skillData is not SkillData skill) continue;
                Sprite iconTarget = await ResourceManager.Instance.LoadAsync<Sprite>(skill.skillIconImage);
                skillIconSpriteMap.TryAdd(skill.skillIconImage, iconTarget);
            }
        }
        
        public void SetCharacterSkillMap()
        {
            string key = nameof(SkillData);
            if (_cache.ContainsKey(key) == false)
                return;
            Dictionary<long, SheetData> skillDict = _cache[key];
            
            if (skillDict == null)
            {
                Debug.LogError($"Map not included in parsing : {key}");
                return;
            }
            
            foreach (var _skillData in skillDict.Values)
            {
                if (_skillData is not SkillData skill) continue;
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
        
        public void SetSkillRangeMap()
        {
            string key = nameof(SkillRangeData);
            if (_cache.ContainsKey(key) == false)
                return;
            Dictionary<long, SheetData> skillDict = _cache[key];
            if (skillDict == null)
            {
                Debug.LogError($"Map not included in parsing : {key}");
                return;
            }

            foreach (var _rangeData in skillDict.Values)
            {
                if (_rangeData is not SkillRangeData range) continue;
                skillRangeMap.TryAdd(range.skillIndex, range);
            }
        }
    }

}
