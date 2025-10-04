using Core.Scripts.Foundation.Define;
using GamePlay.Common.Scripts.Entities.Skills;
using GamePlay.Features.Battle.Scripts.Unit;
using GamePlay.Skill;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace GamePlay.Common.Scripts.Skill
{
    public class SkillParameter
    {
        public CharBase Caster;
        public List<CharBase> Target;
        public SkillModel model;
        public readonly SystemEnum.eSkillType SkillType;
        public readonly float Accuracy;              
        public readonly float CritMultiplier;
        
        public SkillParameter(
            CharBase caster, 
            List<CharBase> target,
            SkillModel model
            )
        {
            Caster = caster;
            Target = target;
            model = model;
        }
    }
    
    [RequireComponent(typeof(SkillMarkerReceiver))]
    public class SkillBase : MonoBehaviour
    {
        private SkillModel _model;
        private PlayableDirector _director;
        public CharBase CharPlayer { get; private set; }
        public SkillParameter SkillParameter;
        public SkillModel SkillModel => _model;
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

        public void Init(SkillModel skillModel)
        {
            _model = skillModel;
        }

        public void SkillPlay(SkillParameter param)
        {
            // 타임라인 재생
            if (_director == null)
                return;
            SkillMarkerReceiver receiver = GetComponent<SkillMarkerReceiver>();
            receiver.Input = param;
            
            
            _director.Play();
        }

    }
}
