using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static SystemEnum;

namespace AngelBeat.Core.SingletonObjects.Managers
{
    /// <summary>
    /// 전투 씬 내에서의 캐릭터를 관리하도록 함.
    /// </summary>
    public class BattleCharManager : SingletonObject<BattleCharManager>
    {
        // 존재하는 Char (Char Type을 Key1 Char ID를 Key2로 사용)
        private Dictionary<Type, Dictionary<long, CharBase>> _cache = new();
        // 고유 ID 생성 
        private long _nextID = 0;
        #region 생성자

        private BattleCharManager() { }
        #endregion
        public long SelectedCharIndex { get; private set; } = 1;
        // 고유 ID 생성
        public long GetNextID() => _nextID++;

        public T GetChar<T>(long ID) where T : CharBase
        {
            var key = typeof(T);

            if (!_cache.ContainsKey(key))
            {
                Debug.LogWarning($"{typeof(T).ToString()} 타입을 찾을 수 없음");
                return null;
            }
            if (_cache[key].ContainsKey(ID))
            {
                Debug.LogWarning($"{key} 타입의 ID: {ID}을 찾을 수 없음");
                return null;
            }
            T findChar = _cache[key][ID] as T;
            if (findChar == null)
            {
                Debug.LogWarning($"{key} 타입의 ID: {ID}을 {key} 타입으로 변환 불가");
                return null;
            }
            return findChar;
        }

        public bool SetChar<T>(T data) where T : CharBase
        {
            var key = typeof(T);
            if (!_cache.ContainsKey(typeof(T)))
            {
                _cache.Add(key, new Dictionary<long, CharBase>());
            }
            if (_cache[key].ContainsKey(data.GetID()))
            {
                Debug.LogWarning($"{key} 타입의 ID: {data.GetID()}가 이미 존재함");
                return false;
            }
            _cache[key].Add(data.GetID(), data);
            return true;
        }

        public bool Clear(Type myType, long id)
        {
            if (!_cache.ContainsKey(myType))
            {
                Debug.LogWarning($"{myType.ToString()} 타입을 찾을 수 없음 삭제 실패");
                return false;
            }
            if (_cache[myType].ContainsKey(id))
            {
                Debug.LogWarning($"{myType.ToString()} 타입의 ID: {id}을 찾을 수 없음 삭제 실패");
                return false;
            }
            var findChar = _cache[myType][id];
            if (findChar == null)
            {
                Debug.LogWarning($"{myType} 타입의 id: {id}을 {myType} 타입으로 변환 불가 삭제 실패");
                return false;
            }
            _cache[myType].Remove(id);
            return true;
        }

        public bool Clear(long id)
        {
            foreach (var typeData in _cache)
            {
                Dictionary<long, CharBase> typeDic = typeData.Value;

                if (typeDic == null)
                    continue;

                if (typeDic.ContainsKey(id))
                {
                    typeDic.Remove(id);
                }
            }

            return true;
        }

        public CharBase CharGenerate(CharParameter charParam)
        {
            CharBase charBase = Instance.CharGenerate(charParam.CharIndex);
            charBase.transform.position = charParam.GeneratePos;
            return charBase;
        }
       
        public CharBase CharGenerate(long charIndex)
        {
            CharData charData = DataManager.Instance.GetData<CharData>(charIndex);
            if (charData == null)
            {
                Debug.LogWarning($"CharFactory : {charIndex} 의 CharIndex를 찾을 수 없음");
                return null;
            }
            GameObject gameObject = ResourceManager.Instance.Instantiate($"Char/{charData.charPrefabName}");
            if (gameObject == null)
            {
                Debug.LogWarning($"CharFactory : {charData.charPrefabName} 캐릭터 프리팹 루트의 charPrefab을 찾을 수 없음");
                return null;
            }

            CharBase charBase = gameObject.GetComponent<CharBase>();
            return charBase;
        }

        
        public CharBase GetFieldChar(long uid)
        {
            foreach (var dict in _cache.Values)
            {
                if (dict.ContainsKey(uid))
                {
                    return dict[uid];
                }
            }
            return null;
        }

        // 누구편인지에 따라 갈라서, 리스트에 넣고 전달한다.(속도로 sorting할 것)
        public List<CharBase> GetBattleParticipants()
        {
            var battleParticipants = new List<CharBase>();
        
            foreach (var kvp in _cache)
            {
                foreach(var unit in kvp.Value)
                {
                    CharBase character = unit.Value;
                    battleParticipants.Add(character);
                }
            }

            return battleParticipants
                .OrderByDescending(x => x.CharStat.GetStat(SystemEnum.eStats.NSpeed))
                .ThenBy(c => c.GetCharType() == eCharType.Enemy)
                .ThenBy(c => c.GetID())
                .ToList();
        }




    }
}