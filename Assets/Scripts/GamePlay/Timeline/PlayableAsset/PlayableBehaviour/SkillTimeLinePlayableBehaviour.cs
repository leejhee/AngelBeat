using Character.Unit;
using GamePlay.Skill;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Playables;

namespace AngelBeat
{
    /// <summary>
    /// ��ų�� �÷��̾�� �ൿ
    /// </summary>
    public abstract class SkillTimeLinePlayableBehaviour : PlayableBehaviour
    {
        public CharBase charBase;
        public SkillBase skillBase;
        
        public virtual void InitBehaviour(CharBase charBase, SkillBase skillBase)
        {
            this.charBase = charBase;
            this.skillBase = skillBase;
        }
    }
}