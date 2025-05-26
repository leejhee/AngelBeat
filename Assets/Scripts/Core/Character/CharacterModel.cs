using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AngelBeat.Core.Character
{
    /// <summary> 파티에는 이 정보가 저장됩니다. </summary>
    [Serializable]
    public class CharacterModel
    {
        [SerializeField]
        private long index;
        private CharData _data;
        private CharStat _stat;
        
        private List<SkillModel> _skillModels = new();
        
        public long Index => index;
        public CharStat Stat => _stat;
        public IReadOnlyList<SkillModel> Skills => _skillModels.AsReadOnly();
        
        // 탐사 중 영입 시 등록
        public CharacterModel(long index)
        {
            this.index = index;
            _data = DataManager.Instance.GetData<CharData>(index);
            if(_data == null)
            {
                Debug.LogError("생성자 중 포함되지 않은 캐릭터데이터에 의한 오류");
                return;
            }
            
            CharStatData stat = DataManager.Instance.GetData<CharStatData>(_data.charStat);
            _stat = new CharStat(stat);

            List<long> skillList = _data.charSkillList;
            foreach (long skill in skillList)
            {
                SkillData skillData = DataManager.Instance.GetData<SkillData>(skill);
                _skillModels.Add(new SkillModel(skillData));
            }
        }
        
        // 기존의 데이터로 등록
        public CharacterModel(CharData data, CharStat stat, IReadOnlyCollection<SkillModel> skills)
        {
            _data = data;
            _stat = stat;
            _skillModels = skills.ToList();
        }
        
    }
    
}