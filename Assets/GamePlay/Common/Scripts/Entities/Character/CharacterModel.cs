using AngelBeat;
using Core.Scripts.Data;
using Core.Scripts.Foundation.Define;
using GamePlay.Common.Scripts.Entities.Skills;
using GamePlay.Skill;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DataManager = Core.Scripts.Managers.DataManager;

namespace GamePlay.Common.Scripts.Entities.Character
{
    /// <summary> 파티에는 이 정보가 저장됩니다. </summary>
    [Serializable]
    public class CharacterModel
    {
        [SerializeField]
        private long index;
        private CharData _data;
        private CharStat _stat;
        private SystemEnum.eCharType _type;
        private List<SkillModel> _skillModels = new();
        
        public long Index => index;
        public string Name => _data.charName;
        public CharStat Stat => _stat;
        public IReadOnlyList<SkillModel> Skills => _skillModels.AsReadOnly();
        public SystemEnum.eCharType characterType => _type;
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

            _type = _data.defaultCharType;
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
    
    /// <summary>
    /// 전역적인 도깨비 모델. party의 초기화 시 자동으로 생성됩니다.
    /// </summary>
    [Serializable]
    public class DokkaebiModel
    {
        private const int DokkaebiIndex = 58776974;
        private readonly DokkaebiData _data;
        private readonly CharStat _stat;
        private readonly SystemEnum.eCharType _type = SystemEnum.eCharType.Player;
        
        public DokkaebiModel()
        {
            _data = DataManager.Instance.GetData<DokkaebiData>(DokkaebiIndex);
            _stat = new CharStat(_data);
        }

        public CharacterModel GetDokkaebiToChar()
        {
            throw new NotImplementedException("GetDokkaebiToChar");
        }
    }
    
}