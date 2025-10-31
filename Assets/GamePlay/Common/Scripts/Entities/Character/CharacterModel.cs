using AngelBeat;
using Core.Scripts.Data;
using Core.Scripts.Foundation.Define;
using Core.Scripts.Managers;
using GamePlay.Common.Scripts.Entities.Character.Components;
using GamePlay.Common.Scripts.Entities.Skills;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GamePlay.Common.Scripts.Entities.Character
{
    /// <summary> 파티에는 이 정보가 저장됩니다. </summary>
    [Serializable]
    public class CharacterModel
    {
        [SerializeField] private long index;
        #region Fields
        private string _characterName;
        private CharStat _baseStat;
        private SystemEnum.eCharType _type;
        
        /// <summary>
        /// 이 캐릭터가 가질 수 있는 모든 스킬의 리스트
        /// TODO : 해금 목록이 따로 드러나야하는지 알아볼 것(현재 확인 불가)
        /// </summary>
        private List<SkillModel> _allSkillModels = new();
        
        /// <summary>
        /// 현재 활성화되어있는 스킬들
        /// TODO : 스킬 연결 앞으로 여기에 할 것.
        /// </summary>
        private List<SkillModel> _activeSkillModels = new();
        
        #region Asset Key Route
        private string _prefabRoot;
        private string _iconSpriteRoot;
        private string _ldRoot;
        #endregion
        #endregion
        
        #region Properties
        public long Index => index;
        public CharStat BaseStat => _baseStat;
        public string Name => _characterName;
        public IReadOnlyList<SkillModel> AllSkills => _allSkillModels.AsReadOnly();
        public IReadOnlyList<SkillModel> ActiveSkills => _activeSkillModels.AsReadOnly();
        public SystemEnum.eCharType CharacterType => _type;

        public string PrefabRoot => _prefabRoot;
        public string IconSpriteRoot => _iconSpriteRoot;
        public string LdRoot => _ldRoot;
        #endregion
        
        
        // 탐사 중 영입 시 등록
        public CharacterModel(CompanionData companion)
        {
            index = companion.index;
            _type = SystemEnum.eCharType.Player;
            _characterName = companion.charPrefabName;
            _prefabRoot = companion.charPrefabName;
            _iconSpriteRoot = companion.charImage;
            _ldRoot = companion.charLDRoute;

            long statIndex = companion.charStatID;
            CharStatData statData = DataManager.Instance.GetData<CharStatData>(statIndex);
            if (statData == null)
            {
                Debug.LogError($"[CharacterModel] : Invalid Stat Data - Check your Index : {statIndex}");
                return;
            }
            _baseStat = new CharStat(statData);

            //스킬 - 동료 나중
            
        }
        
        public CharacterModel(MonsterData monster)
        {
            index = monster.index;
            _type = SystemEnum.eCharType.Enemy;
            _characterName = monster.charName;
            _prefabRoot = monster.charPrefabName;
            _iconSpriteRoot = monster.charImage;
            _ldRoot = null;

            long statIndex = monster.charStatID;
            CharStatData statData = DataManager.Instance.GetData<CharStatData>(statIndex);
            if (statData == null)
            {
                Debug.LogError($"[CharacterModel] : Invalid Stat Data - Check your Index : {statIndex}");
                return;
            }
            _baseStat = new CharStat(statData);
            
            // 몬스터는 데이터 상의 스킬 사용에 제한이 없다고 가정
            var skills = DataManager.Instance.CharacterSkillMap[index];
            foreach(var sk in skills)
                _allSkillModels.Add(new SkillModel(sk));
            _activeSkillModels = new List<SkillModel>(_allSkillModels);
        }
        
        /// <summary>
        /// 파티 초기화 시 필요.
        /// </summary>
        public CharacterModel(DokkaebiData dok)
        {
            index = dok.index;
            _type = SystemEnum.eCharType.Player;
            _characterName = dok.DokkaebiName;
            _ldRoot = dok.SpriteLDRoute;
            _iconSpriteRoot = dok.SpriteIconRoute;
            _prefabRoot = dok.PrefabRoot;
            
            _baseStat = new CharStat(dok);
            
            // TODO : 초기화 테이블 따로 가지고 있게 할 것
            var mungeData = DataManager.Instance.GetData<DokkaebiSkillData>(10101001);
            var smokeData = DataManager.Instance.GetData<DokkaebiSkillData>(10101002);
            var mungeModel = new SkillModel(mungeData);
            var smokeModel = new SkillModel(smokeData);
            _allSkillModels.Add(mungeModel); _allSkillModels.Add(smokeModel);
            _activeSkillModels = new List<SkillModel>(_allSkillModels);
        }
        
        // 기존의 데이터로 등록
        public CharacterModel(CharacterModel model)
        {
            index = model.Index;
            _type = model.CharacterType;
            _characterName = model.Name;
            _ldRoot = model.LdRoot;
            _iconSpriteRoot = model.IconSpriteRoot;
            _allSkillModels = model._allSkillModels;
            _baseStat = model.BaseStat;
        }

        public void AddSkill(SkillModel skillModel)
        {
            if (_activeSkillModels.Contains(skillModel) || _allSkillModels.Contains(skillModel))
            {
                Debug.LogWarning("이미 있는 스킬 데이터입니다.");
                return;
            }
            
            _allSkillModels.Add(skillModel);
            if (_activeSkillModels.Count < 4)
            {
                _activeSkillModels.Add(skillModel);
            }
        }
        
    }
    
}