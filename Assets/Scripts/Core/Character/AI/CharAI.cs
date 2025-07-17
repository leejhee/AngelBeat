using Modules.BT;
using Modules.BT.Nodes;
using System;

namespace AngelBeat
{
    public class CharAI
    {
        private BTNode _root;
        private BTContext _context;
        
        public CharAI(BTContext context)
        {
            _context = context;
            _root = BuildMockTree();
        }
        
        //public IEnumerator AIRoutine()
        //{
        //    while (_isMovable || _isAttackable)
        //    {
        //        if (!CurrentTarget)
        //        {
        //            CurrentTarget = BattleCharManager.Instance.GetNearestEnemy(CharAgent);
        //            yield return null;
        //        }
        //        else if (_reloaded == null)
        //        {
        //            var skills = CharAgent.CharInfo.Skills;
        //            _reloaded = skills[Random.Range(0, skills.Count)];
        //            yield return null;
        //        }
        //        else
        //        {
        //            int skillRange = _reloaded.SkillRange;
        //            
        //            Vector3 targetDir = CurrentTarget.transform.position - CharAgent.transform.position;
        //            if (targetDir.magnitude > skillRange)
        //            {
        //                if (targetDir.y < -1f && CharAgent.IsGrounded)
        //                {
        //                    //yield return CharAgent.StartCoroutine(CharAgent.CharDownJump());
        //                    EventBus.Instance.SendMessage(new OnTurnEndInput());
        //                }
        //                else if (targetDir.y > 1f && CharAgent.IsGrounded)
        //                {
        //                    //yield return CharAgent.StartCoroutine(CharAgent.CharJump());
        //                    EventBus.Instance.SendMessage(new OnTurnEndInput());
        //                }
        //                else
        //                {
        //                    Vector3 dir = targetDir.normalized;
        //                    CharAgent.CharMove(dir.x > 0 ? Vector3.right : Vector3.left);
        //                    yield return null;
        //                }
        //            }
        //            else
        //            {
        //                //if (AgentStat.UseActionPoint(SystemConst.fps))
        //                //{
        //                //    CharAgent.SkillInfo.PlaySkill(_reloaded.SkillIndex, new SkillParameter(
        //                //        CharAgent, new List<CharBase> { CurrentTarget }));
        //                //    
        //                //    yield return new WaitUntil(() => 
        //                //        CharAgent.SkillInfo.GetPlayingTimeline(_reloaded.SkillIndex).state != PlayState.Playing);
        //                //    _reloaded = null;
        //                //}
        //            }
        //        }
        //    }
        //    EventBus.Instance.SendMessage(new OnTurnEndInput());
        //}

        //private void UpdateAIState(SystemEnum.eStats stat, long changed)
        //{
        //    if (stat == SystemEnum.eStats.NACTION_POINT)
        //    {
        //        _isMovable = changed > 0;
        //        _isAttackable = changed >= SystemConst.fps;
        //    }
        //}

        private BTNode BuildMockTree()
        {
            throw new NotImplementedException("좀만 기다려봐~");
        }

    }
}
