using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace AngelBeat
{
    public class SkillParameter
    {
        public CharBase Caster;
        public List<CharBase> Target;
        public float DamageCalibration;
        public float Accuracy;
        public float CritMultiplier;
        
        public SkillParameter(
            CharBase caster, 
            List<CharBase> target,
            float damageCalibration = 1,
            float accuracy = 100,
            float critMultiplier = 1)
        {
            Caster = caster;
            Target = target;
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
