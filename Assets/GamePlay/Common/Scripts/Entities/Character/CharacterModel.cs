using AngelBeat;
using Core.Scripts.Data;
using Core.Scripts.Foundation.Define;
using GamePlay.Common.Scripts.Entities.Skills;
using System;
using System.Collections.Generic;
using UnityEngine;
using DataManager = Core.Scripts.Managers.DataManager;



namespace GamePlay.Common.Scripts.Entities.Character
{
    /// <summary> 파티에는 이 정보가 저장됩니다. </summary>
    [Serializable]
    public class CharacterModel
    {
        [SerializeField] private long index;
        private CharStat _stat;
        private SystemEnum.eCharType _type;
        private List<SkillModel> _skillModels = new();
        private string _characterName;
        
        #region Asset Key Route
        private string _prefabRoot;
        private string _iconSpriteRoot;
        private string _ldRoot;
        #endregion
        
        public long Index => index;
        public CharStat Stat => _stat;
        public string Name => _characterName;
        public IReadOnlyList<SkillModel> Skills => _skillModels.AsReadOnly();
        public SystemEnum.eCharType CharacterType => _type;

        public string PrefabRoot => _prefabRoot;
        public string IconSpriteRoot => _iconSpriteRoot;
        public string LdRoot => _ldRoot;
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
            _stat = new CharStat(statData);

            //스킬
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
            _stat = new CharStat(statData);
            //스킬
        }
        
        /// <summary>
        /// 파티 초기화 시 필요.
        /// </summary>
        /// <param name="dok"></param>
        public CharacterModel(DokkaebiData dok)
        {
            index = dok.index;
            _type = SystemEnum.eCharType.Player;
            _characterName = dok.DokkaebiName;
            _ldRoot = dok.SpriteLDRoute;
            _iconSpriteRoot = dok.SpriteIconRoute;
            _prefabRoot = dok.PrefabRoot;
            
            _stat = new CharStat(dok);
            
            //스킬
        }
        
        // 기존의 데이터로 등록
        public CharacterModel(CharacterModel model)
        {
            index = model.Index;
            _type = model.CharacterType;
            _characterName = model.Name;
            _ldRoot = model.LdRoot;
            _iconSpriteRoot = model.IconSpriteRoot;
            _skillModels = model._skillModels;
            _stat = model.Stat;
        }
        
    }
    
}