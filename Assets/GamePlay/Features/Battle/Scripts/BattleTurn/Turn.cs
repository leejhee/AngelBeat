using Core.Scripts.Foundation.Define;
using Cysharp.Threading.Tasks;
using GamePlay.Features.Battle.Scripts.Unit;
using System;
using UnityEngine;

namespace GamePlay.Features.Battle.Scripts.BattleTurn
{
    public class Turn
    {
        public enum Side { None, Player, Enemy, Neutral, SideMax }

        public CharBase TurnOwner { get; private set; }
        public Side WhoseSide { get; private set; }
        public bool IsValid => TurnOwner;
        
        public TurnActionState ActionState { get; private set; }
        
        private bool _isDead = false;
        private readonly Action _onBeginTurn =     delegate { };
        private readonly Action _onEndTurn =       delegate { };
        
        public event Action OnAITurnCompleted;
        
        public Turn(CharBase turnOwner)
        {
            TurnOwner = turnOwner;
            WhoseSide = turnOwner.GetCharType() == SystemEnum.eCharType.Enemy ?
                Side.Enemy : Side.Player;
            
            ActionState = new TurnActionState();
            
            _onBeginTurn += DefaultTurnBegin;
            _onEndTurn += DefaultTurnEnd;
        }
        
        public void Begin() => _onBeginTurn();
        public void End() => _onEndTurn();

        private void DefaultTurnBegin()
        {
            float movePoint = TurnOwner.RuntimeStat.GetStat(SystemEnum.eStats.NMACTION_POINT);
            ActionState.Initialize(movePoint);
            
            // Control Camera.
            FocusCamera();
                
            #region Control Logic
            if (TurnOwner.GetCharType() == SystemEnum.eCharType.Enemy)
            {
                //Debug.Log("Monster turn : AI not implemented");
                CharMonster monster = TurnOwner as CharMonster;
                if (monster)
                {
                    Debug.Log($"[AI] {monster.name} AI 실행 시작");
                    ExecuteMonsterAI(monster).Forget();
                }
                else
                {
                    Debug.LogWarning($"[Turn] {TurnOwner.name}은 CharMonster가 아닙니다.");
                }
            }
            
            //TurnOwner.KeywordInfo.ExecuteByPhase(SystemEnum.eExecutionPhase.SoT, TriggerType.EoT);
            #endregion
        }
        
        private async UniTaskVoid ExecuteMonsterAI(CharMonster monster)
        {
            try
            {
                await monster.ExecuteAITurn(this);
                Debug.Log($"[Turn] {monster.name} AI 행동 완료");
                
                TurnActionUtility.LogActionState(monster.name, ActionState);  // 로깅
                
                await UniTask.Delay(500);  // 연출용 딜레이
                Debug.Log($"[Turn] {monster.name} AI 턴 종료");
                
                End();  // 턴 종료 처리
                OnAITurnCompleted?.Invoke();  // 다음 턴으로 넘어가라고 알림
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                Debug.LogError($"[Turn] {monster.name} AI 실행 중 오류 발생, 강제 턴 종료");
                
                End();
                OnAITurnCompleted?.Invoke();
                
            }
        }
        
        private void DefaultTurnEnd()
        {
            FocusCamera();
            if (TurnOwner.GetCharType() == SystemEnum.eCharType.Enemy)
            {
                Debug.Log($"[Turn] {TurnOwner.name} 적 턴 종료 처리");
            }
            else
            {
                Debug.Log($"[Turn] {TurnOwner.name} 플레이어 턴 종료 처리");
            }
            //TurnOwner.KeywordInfo.ExecuteByPhase(SystemEnum.eExecutionPhase.EoT, TriggerType.EoT);
        }

        private void FocusCamera()
        {
            float z = TurnOwner.MainCamera.transform.position.z;
            Vector3 charPos = TurnOwner.CharTransform.position;
            TurnOwner.MainCamera.transform.position = new Vector3(charPos.x, charPos.y, z);
        }
        
        /// <summary>
        /// 행동 수행 가능 여부 검증
        /// </summary>
        public bool CanPerformAction(TurnActionState.ActionCategory category, int moveDistance=0)
        {
            return ActionState.CanPerformAction(category, moveDistance);
        }
        
        /// <summary>
        /// 이동 실행 (이동력 소모)
        /// </summary>
        public bool TryConsumeMove(float distance)
        {
            return ActionState.ConsumeMovePoint(distance);
        }
        
        /// <summary>
        /// 주요 행동 실행 (밀기/점프/스킬)
        /// </summary>
        public bool TryUseMajorAction()
        {
            return ActionState.UseMajorAction();
        }
    }
}
