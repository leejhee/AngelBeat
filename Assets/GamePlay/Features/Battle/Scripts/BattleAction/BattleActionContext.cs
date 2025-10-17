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
        Skill
    }
    
    [Serializable]
    public sealed class BattleActionContext
    {
        //=========== 전투 행동 타입 ============//
        public ActionType battleActionType;
        
        //=========== 행동 주체 / 대상 ============//
        public CharBase actor;
        public List<CharBase> targets;
        
        //=========== 전투 환경 ============//
        public StageField battleField;
        public Vector2Int targetCell;
        public List<Vector2Int> effectRangeCells;
        
        //=========== 외부 Cancel Token ============//
        public CancellationToken ExternalToken;
    }

    public readonly struct BattlePreviewData
    {
        public readonly List<Vector2Int> possiblePositions;
        public readonly List<Vector2Int> blockedPositions;
    }
    
}