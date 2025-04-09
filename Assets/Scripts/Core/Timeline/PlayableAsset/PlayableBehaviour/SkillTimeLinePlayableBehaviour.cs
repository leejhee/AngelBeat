using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Playables;

namespace Client
{
    /// <summary>
    /// 스킬용 플레이어블 행동
    /// </summary>
    public abstract class SkillTimeLinePlayableBehaviour : PlayableBehaviour
    {
        public CharBase charBase;
        public SkillBase skillBase;
    }
}