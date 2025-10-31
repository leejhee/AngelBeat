﻿using Cysharp.Threading.Tasks;
using GamePlay.Features.Battle.Scripts.BattleMap;
using GamePlay.Features.Battle.Scripts.Unit;
using System.Threading;
using UnityEngine;

namespace GamePlay.Features.Battle.Scripts.BattleAction
{
    /// <summary>
    /// 푸시/넉백 규칙. 
    /// </summary>
    public static class PushEngine
    {
        public enum VictimResult{ Fall, Land, JustPush, WallSmack }
        public struct PushResult
        {
            public Vector2Int FirstGoal;
            public Vector2Int? LandingGoal;
            public int FallCells;
            public VictimResult Result;
        }

        public static PushResult ComputePushResult(
            Vector2Int pivot,
            Vector2Int target,
            BattleStageGrid grid)
        {
            PushResult result = new();
            Vector2Int dir = target - pivot;
            Vector2Int goal = target + dir;
            
            result.FirstGoal = goal;

            if (!grid.IsPlatform(goal))
            {
                if (!grid.TryFindLandingFloor(goal, out Vector2Int landing, out int fallCells))
                {
                    result.LandingGoal = new Vector2Int(goal.x, -1);
                    result.Result = VictimResult.Fall;
                    return result;
                }
                
                result.LandingGoal = landing;
                result.FallCells = fallCells;
                result.Result = VictimResult.Land;
                return result;
            }

            if(grid.IsOccupied(goal) || grid.IsObstacle(goal))
            {
                result.FirstGoal = target;
                result.LandingGoal = null;
                result.Result = VictimResult.WallSmack;
                return result;
            }

            result.LandingGoal = null;
            result.Result = VictimResult.JustPush;
            return result;
        }

        public static async UniTask ApplyPushResult(
            CharBase victim,
            PushResult result,
            BattleStageGrid grid,
            CancellationToken ct,
            bool playAnimation = false,
            bool suppressIdle = false)
        {
            if (result.Result == VictimResult.JustPush)
            {
                await victim.CharKnockBack(
                    grid.CellToWorldCenter(result.FirstGoal), playAnimation, suppressIdle);
                grid.MoveUnit(victim, result.FirstGoal);
            }
            else if (result.Result == VictimResult.Land)
            {
                await victim.CharKnockBack(
                    grid.CellToWorldCenter(result.FirstGoal), playAnimation, suppressIdle); // 먼저 한번 밀쳐지고
                await victim.CharKnockBack(grid.CellToWorldCenter(
                    result.LandingGoal.GetValueOrDefault()), playAnimation, suppressIdle); //떨어지기
                // 다 떨어지면 피해 계산
                grid.MoveUnit(victim, result.LandingGoal.GetValueOrDefault());
                victim.RuntimeStat.ReceiveHPPercentDamage(30 * result.FallCells);
            }
            else if (result.Result == VictimResult.Fall)
            {
                await victim.CharKnockBack(
                    grid.CellToWorldCenter(result.FirstGoal), playAnimation, suppressIdle); // 먼저 한번 밀쳐지고
                await victim.CharKnockBack(
                    grid.CellToWorldCenter(result.LandingGoal.GetValueOrDefault()), playAnimation, suppressIdle); //떨어지기
                //낙사 처리
                grid.RemoveUnit(victim);
                victim.CharDead();
            }
            else
            {
                //반칸 밀렸다가 작은 포물선으로 돌아오게 수정
                await victim.CharKnockBack(
                    grid.CellToWorldCenter(result.FirstGoal), playAnimation, suppressIdle);
                victim.RuntimeStat.ReceiveHPPercentDamage(30); // 30퍼만
            }
        }
    }
}