using GamePlay.Common.Scripts.Entities.Skills;
using GamePlay.Features.Battle.Scripts.BattleMap;
using GamePlay.Features.Battle.Scripts.Unit;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace GamePlay.Features.Battle.Scripts.BattleAction
{
    public enum ActionType
    {
        None,
        Move,
        Jump,
        Push,
        Skill
    }
    
    [Serializable]
    public sealed class BattleActionContext
    {
        //=========== 전투 행동 타입 ============//
        public ActionType battleActionType;
        
        //=========== 행동 주체 ============//
        public CharBase actor;
        
        //=========== 전투 환경 ============//
        public StageField battleField;

        public Vector2Int? TargetCell;
        
        //=========== Action 중 별개 필요 요소 ============//
        public SkillModel skillModel;
        
        //=========== 외부 Cancel Token ============//
        public CancellationToken ExternalToken;
    }
    
    public readonly struct BattleActionPreviewData
    {
        public readonly List<Vector2Int> PossibleCells;
        public readonly List<Vector2Int> BlockedCells;

        public BattleActionPreviewData(List<Vector2Int> possibleCells, List<Vector2Int> blockedCells)
        {
            PossibleCells = possibleCells;
            BlockedCells = blockedCells;
        }
    }

    public readonly struct BattleActionResult
    {
        public enum ResultReason
        {
            None,
            InvalidTarget,
            InvalidContext,
        }

        public BattleActionResult(bool success, ResultReason reason = ResultReason.None)
        {
            ActionSuccess = success;
            Reason = reason;
        }

        public readonly bool ActionSuccess;
        public readonly ResultReason Reason;

        public static BattleActionResult Success() => new(true);
        public static BattleActionResult Fail(ResultReason reason) => new(false, reason);

    }
    
}