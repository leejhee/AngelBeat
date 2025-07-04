using AngelBeat.Core.SingletonObjects.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace AngelBeat
{
    public class CharAI
    {
        public CharBase CharAgent { get; private set; }
        public CharStat AgentStat => CharAgent.CharStat;
        public CharBase CurrentTarget { get; private set; }
        
        private bool _isAIOn;
        private bool _isMovable = true;
        private bool _isAttackable = true;
        private SkillModel _reloaded;
        public CharAI(CharBase charAgent)
        {
            CharAgent = charAgent;
            AgentStat.OnStatChanged += UpdateAIState;
        }
        
        public IEnumerator AIRoutine()
        {
            while (_isMovable || _isAttackable)
            {
                if (!CurrentTarget)
                {
                    CurrentTarget = BattleCharManager.Instance.GetNearestEnemy(CharAgent);
                    yield return null;
                }
                else if (_reloaded == null)
                {
                    var skills = CharAgent.CharInfo.Skills;
                    _reloaded = skills[Random.Range(0, skills.Count)];
                    yield return null;
                }
                else
                {
                    int skillRange = _reloaded.SkillRange;
                    
                    Vector3 targetDir = CurrentTarget.transform.position - CharAgent.transform.position;
                    if (targetDir.magnitude > skillRange)
                    {
                        if (targetDir.y < -1f && CharAgent.IsGrounded)
                        {
                            //yield return CharAgent.StartCoroutine(CharAgent.CharDownJump());
                            EventBus.Instance.SendMessage(new OnTurnEndInput());
                        }
                        else if (targetDir.y > 1f && CharAgent.IsGrounded)
                        {
                            //yield return CharAgent.StartCoroutine(CharAgent.CharJump());
                            EventBus.Instance.SendMessage(new OnTurnEndInput());
                        }
                        else
                        {
                            Vector3 dir = targetDir.normalized;
                            CharAgent.CharMove(dir.x > 0 ? Vector3.right : Vector3.left);
                            yield return null;
                        }
                    }
                    else
                    {
                        //if (AgentStat.UseActionPoint(SystemConst.fps))
                        //{
                        //    CharAgent.SkillInfo.PlaySkill(_reloaded.SkillIndex, new SkillParameter(
                        //        CharAgent, new List<CharBase> { CurrentTarget }));
                        //    
                        //    yield return new WaitUntil(() => 
                        //        CharAgent.SkillInfo.GetPlayingTimeline(_reloaded.SkillIndex).state != PlayState.Playing);
                        //    _reloaded = null;
                        //}
                    }
                }
            }
            EventBus.Instance.SendMessage(new OnTurnEndInput());
        }

        private void UpdateAIState(SystemEnum.eStats stat, long changed)
        {
            if (stat == SystemEnum.eStats.NACTION_POINT)
            {
                _isMovable = changed > 0;
                _isAttackable = changed >= SystemConst.fps;
            }
        }
        
    }
}
