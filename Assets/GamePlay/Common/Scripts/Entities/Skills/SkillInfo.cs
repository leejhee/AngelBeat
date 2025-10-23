using Core.Scripts.Data;
using Core.Scripts.Foundation.Utils;
using Cysharp.Threading.Tasks;
using GamePlay.Common.Scripts.Skill;
using GamePlay.Features.Battle.Scripts.Unit;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace GamePlay.Common.Scripts.Entities.Skills
{
    public class SkillInfo
    {
        private Dictionary<long, SkillBase> _dicSkill = new Dictionary<long, SkillBase>(); // 스킬 리스트
        private List<SkillModel> _skillSlots = new();
        private CharBase _charBase; // 스킬 시전자
        private Transform _SkillRoot; // 스킬 루트 
        public IReadOnlyList<SkillModel> SkillSlots => _skillSlots.AsReadOnly();
        public PlayableDirector GetPlayingTimeline(long skillIndex) =>
            _dicSkill[skillIndex].GetComponent<PlayableDirector>();
        
        public SkillInfo(CharBase charBase)
        {
            _charBase = charBase;
        }
        
        #region Initialization
        /// <summary>
        /// Char Start 시점
        /// </summary>
        /// <param name="skillArray"></param>
        [Obsolete]
        public void Init(List<long> skillArray)
        {
            // 스킬 루트 오브젝트 제작
            string SkillRoot = "SkillRoot";
            GameObject skillRoot = Util.FindChild(_charBase.gameObject, SkillRoot, false);
            if (skillRoot == null)
            {
                skillRoot = new GameObject(SkillRoot);
            }
            _SkillRoot = skillRoot.transform;

            // 스킬 추가
            for (int i = 0; i < skillArray.Count; i++)
            {
                AddSkill(skillArray[i]);
            }
        }
        
        [Obsolete]
        public void Init(List<SkillData> skills)
        {
            string SkillRoot = "SkillRoot";
            GameObject skillRoot = Util.FindChild(_charBase.gameObject, SkillRoot, false);
            if (skillRoot == null)
            {
                skillRoot = new GameObject(SkillRoot);
            }
            _SkillRoot = skillRoot.transform;
            
            for (int i = 0; i < skills.Count; i++)
            {
                AddSkill(skills[i]);
            }
        }

        public async UniTask InitAsync(IReadOnlyList<SkillModel> activeSkills)
        {
            // SkillRoot 보장하기
            string skillRoot = "SkillRoot";
            GameObject root = Util.FindChild(_charBase.gameObject, skillRoot, false);
            if (root == null)
            {
                root = new GameObject(skillRoot);
            }
            _SkillRoot = root.transform;
            
            // 런타임 할당 및 스킬 딕셔너리 초기화
            foreach(SkillModel skill in activeSkills) _skillSlots.Add(skill);
            foreach (SkillModel t in _skillSlots)
            {
                await AddSkill(t);
            }
        }
        #endregion
        
        #region Skill Prefab Managing
        public void DeleteSkill(long skillIndex)
        {
            if (_dicSkill == null)
                return;

            if (_dicSkill.ContainsKey(skillIndex))
            {
                _dicSkill.Remove(skillIndex);
            }
        }

        public void AddSkill(long skillIndex)
        {
            if (_dicSkill == null)
                return;
            
            SkillBase skillBase = SkillFactory.CreateSkill(skillIndex);
            if (skillBase == null)
                return;

            skillBase.SetCharBase(_charBase);
            _dicSkill.Add(skillIndex, skillBase);
            skillBase.transform.parent = _SkillRoot;
            
        }

        public void AddSkill(SkillData skillData)
        {
            if (_dicSkill == null)
                return;
            
            SkillBase skillBase = SkillFactory.CreateSkill(skillData);
            if (skillBase == null)
                return;
            
            skillBase.SetCharBase(_charBase);
            _dicSkill.Add(skillData.index, skillBase);
            skillBase.transform.parent = _SkillRoot;
        }

        public async UniTask AddSkill(SkillModel skillModel)
        {
            if (_dicSkill == null) return;
            SkillBase skillBase = await SkillFactory.CreateSkill(skillModel);

            if (!skillBase) return;
            
            skillBase.SetCharBase(_charBase);
            _dicSkill.Add(skillModel.SkillIndex, skillBase);
            skillBase.transform.parent = _SkillRoot;
        }
        
        #endregion

        public void PlaySkill(int skillSlotIndex, SkillParameter parameter)
        {
            Debug.Log($"[SkillInfo] Playing Skill By Slot number {skillSlotIndex}");
            if (_dicSkill == null)
            {
                Debug.LogWarning($"[SkillInfo] Skill Dictionary is null");
                return;
            }   
            if (skillSlotIndex < 0 || skillSlotIndex >= _dicSkill.Count)
                throw new OperationCanceledException(
                    $"[SkillInfo] Invalid Skill Slot Index {skillSlotIndex} in {_skillSlots.Count} Slots");
            
            SkillModel model = _skillSlots[skillSlotIndex];
            if (_dicSkill.ContainsKey(model.SkillIndex))
            {
                Debug.Log($"Skill Played : {model.SkillIndex}");
                _dicSkill[model.SkillIndex].SkillPlay(parameter);
            }
        }
        
        public void PlaySkill(long skillIndex, SkillParameter parameter)
        {
            Debug.Log("PlaySkill");
            
            if (_dicSkill == null)
                return;

            if (_dicSkill.ContainsKey(skillIndex))
            {
                Debug.Log($"Skill Played : {skillIndex}");
                _dicSkill[skillIndex].SkillPlay(parameter);
            }
        }
    }
}
