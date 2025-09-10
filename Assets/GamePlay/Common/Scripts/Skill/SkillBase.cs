using Core.Scripts.Data;
using Core.Scripts.Foundation.Define;
using GamePlay.Features.Scripts.Battle.Unit;
using GamePlay.Skill;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace GamePlay.Features.Scripts.Skill
{
    public class SkillParameter
    {
        public CharBase Caster;
        public List<CharBase> Target;
        public readonly SystemEnum.eSkillType SkillType;
        public readonly float DamageCalibration;     
        public readonly float Accuracy;              
        public readonly float CritMultiplier;

        /// <param name="caster">시전자</param>
        /// <param name="target">타겟 캐릭터</param>
        /// <param name="skillType">스킬 타입</param>
        /// <param name="damageCalibration">스킬 대미지 보정계수</param>
        /// <param name="accuracy">스킬 명중률</param>
        /// <param name="critMultiplier">스킬 치명타율</param>
        public SkillParameter(
            CharBase caster, 
            List<CharBase> target,
            SystemEnum.eSkillType skillType,
            float damageCalibration = 1,
            float accuracy = 100,
            float critMultiplier = 1)
        {
            Caster = caster;
            Target = target;
            SkillType = skillType;
            DamageCalibration = damageCalibration;
            Accuracy = accuracy;
            CritMultiplier = critMultiplier;
        }
    }
    
    [RequireComponent(typeof(SkillMarkerReceiver))]
    public class SkillBase : MonoBehaviour
    {
        private SkillData _skillData;
        private PlayableDirector _director;
        public CharBase CharPlayer { get; private set; }
        public SkillParameter SkillParameter;
        private void Awake()
        {
            _director = GetComponent<PlayableDirector>();
            if (_director == null)
            {
                Debug.LogError($"{transform.name} PlayableDirector is Null");
            }
        }
        public void SetCharBase(CharBase charBase)
        {
            CharPlayer = charBase;
        }

        public void Init(SkillData skillData)
        {
            _skillData = skillData;
        }

        public void SkillPlay(SkillParameter param)
        {
            if (_director == null)
                return;
            SkillMarkerReceiver receiver = GetComponent<SkillMarkerReceiver>();
            receiver.Input = param;
            _director.Play();
        }

    }
}
