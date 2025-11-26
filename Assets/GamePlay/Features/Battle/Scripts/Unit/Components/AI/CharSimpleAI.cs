using Cysharp.Threading.Tasks;
using GamePlay.Common.Scripts.Contracts;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay.Features.Battle.Scripts.Unit.Components.AI
{
    public class CharSimpleAI : CharacterAI
    {
        private AIContext _context;
        private AIActionSetGenerator _setGenerator;

        public CharSimpleAI(CharBase owner) : base(owner)
        {
        }
        
        /// <summary>
        /// 작동 구현부
        /// </summary>
        protected override async UniTask ExecuteTurnInternal()
        {
            await UniTask.Delay(1000); // 연출용

            Debug.Log($"[AI] ====== {Owner.name} 턴 시작 ======");

            _context = new AIContext(Owner, Grid);
            _context.AnalyzeSituation();
            //Debug.Log(_context.GetSummary());

            _setGenerator = new AIActionSetGenerator(_context);
            List<AIActionSet> allSets = _setGenerator.GenerateAllActionSets();

            // 3. 재이동 설정
            foreach (var set in allSets)
                _setGenerator.CheckAfterMoveForSet(set);

            // 4. 유효성 필터
            List<AIActionSet> validSets = _setGenerator.FilterInvalidSets(allSets);
            Debug.Log($"[AI] 유효한 세트: {validSets.Count}/{allSets.Count}");

            if (validSets.Count == 0)
            {
                Debug.LogWarning($"[AI] {Owner.name} 실행 가능한 행동 없음, 턴 종료");
                await UniTask.Delay(500);
                return;
            }

            // 5. 가중치
            foreach (var set in validSets)
                _setGenerator.CalculateWeight(set);

            // 6. 상위 세트 선택
            List<AIActionSet> topSets = _setGenerator.SelectTopSets(validSets, 3);

            // 7. 실행
            bool actionSuccess = false;
            foreach (var set in topSets)
            {
                Debug.Log($"[AI] 시도: {set}");
                actionSuccess = await TryExecuteActionSet(set);

                if (actionSuccess)
                {
                    Debug.Log($"[AI] ✓ 성공: {set}");
                    break;
                }

                Debug.Log("[AI] ✗ 실패, 다음 후보 시도");
            }

            if (!actionSuccess)
                Debug.LogWarning($"[AI] {Owner.name} 모든 행동 실패, 턴 종료");

            await UniTask.Delay(500); // 연출용

            Debug.Log($"[AI] ====== {Owner.name} 턴 종료 ======");
        }

        /// <summary>
        /// ActionSet을 실제 행동으로 넘기는 어댑터
        /// </summary>
        private async UniTask<bool> TryExecuteActionSet(AIActionSet set)
        {
            try
            {
                // 0. 방향 전환
                if (set.TargetChar && set.TargetCell.HasValue)
                    AdjustDirection(set.TargetCell.Value);

                // 1. 이동
                if (set.MoveTo.HasValue)
                {
                    bool moveSuccess = await ExecuteMove(set.MoveTo.Value);
                    if (!moveSuccess)
                    {
                        Debug.LogWarning("[AI] 초기 이동 실패");
                        return false;
                    }
                }

                // 2. 메인 행동
                bool actionSuccess = true;

                switch (set.AIActionType)
                {
                    case AIActionType.Attack:
                        {
                            if (set.SkillToUse == null || !set.TargetChar || !set.TargetCell.HasValue)
                            {
                                Debug.LogWarning("[AI] 공격 정보 불완전");
                                return false;
                            }

                            actionSuccess = await ExecuteSkill(
                                set.SkillToUse,
                                set.TargetCell.Value,
                                new List<IDamageable> { set.TargetChar });

                            break;
                        }

                    case AIActionType.Push:
                        {
                            if (!set.TargetCell.HasValue)
                            {
                                Debug.LogWarning("[AI] 푸시 대상 없음");
                                return false;
                            }

                            actionSuccess = await ExecutePush(set.TargetCell.Value);
                            break;
                        }

                    case AIActionType.Jump:
                        {
                            if (!set.TargetCell.HasValue)
                            {
                                Debug.LogWarning("[AI] 점프 대상 없음");
                                return false;
                            }

                            actionSuccess = await ExecuteJump(set.TargetCell.Value);
                            break;
                        }

                    case AIActionType.Move:
                        // 이동만 했으면 이미 끝
                        actionSuccess = true;
                        break;

                    case AIActionType.Wait:
                        await UniTask.Delay(300);
                        actionSuccess = true;
                        break;
                }

                if (!actionSuccess)
                {
                    Debug.LogWarning("[AI] 행동 실행 실패");
                    return false;
                }

                // 3. 재이동
                if (set.AfterMove.HasValue)
                {
                    bool afterMoveSuccess = await ExecuteMove(set.AfterMove.Value);
                    if (!afterMoveSuccess)
                        Debug.LogWarning("[AI] 재이동 실패 (행동은 성공)");
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }
        }
    }
}
