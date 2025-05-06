using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace AngelBeat
{
    public class SkillParameter
    {

    }


    public class SkillBase : MonoBehaviour
    {
        private SkillData _skillData;
        private PlayableDirector _director;
        public CharBase CharPlayer { get; private set; }

   

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

            _director.Play();
        }

    }
}
