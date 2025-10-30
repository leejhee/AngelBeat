﻿using Core.Scripts.Data;
using Core.Scripts.Foundation.Define;
using GamePlay.Features.Battle.Scripts.BattleMap;
using GamePlay.Features.Battle.Scripts.Unit;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay.Features.Battle.Scripts.BattleAction
{
    public static class SkillRangeHelper
    {
        private static bool IsMaskMatch(BattleStageGrid grid, Vector2Int coord, SystemEnum.eCharType maskType)
        {
            if (!grid.IsOccupied(coord) || maskType == SystemEnum.eCharType.None) return false;
            CharBase candidate = grid.GetUnitAt(coord);
            if (!candidate) return false;
            return candidate.GetCharType() == maskType;
        }
        
        public static BattleActionPreviewData ComputeSkillRange(
            BattleStageGrid grid, 
            SkillRangeData rangeData,
            CharBase client)
        {
            List<Vector2Int> targetable = new(); // 지정 가능
            List<Vector2Int> unable = new();    // 지정 불가
            
            // Current Position
            Vector2Int origin = grid.WorldToCell(client.CharTransform.position);
            
            // Inspect Mask
            SystemEnum.eCharType clientType = client.GetCharType();
            SystemEnum.ePivot skillPivot = rangeData.skillPivot;
            
            SystemEnum.eCharType maskType;
            if (skillPivot == SystemEnum.ePivot.TARGET_ENEMY)
                maskType = BattleCharManager.GetEnemyType(clientType);
            else if (skillPivot == SystemEnum.ePivot.TARGET_ALLY)
                maskType = clientType;
            else
                maskType = SystemEnum.eCharType.None;
            
            // Special case
            if (skillPivot == SystemEnum.ePivot.TARGET_SELF)
            {
                targetable.Add(origin); // 만약 나 자신이라면 그냥 나만 넣고 끝내면 됨.
                return new BattleActionPreviewData(targetable, unable);
            }
            
            // Calculation
            
            // '앞' 방향 구별
            Vector2Int forward = client.LastDirection ? Vector2Int.right : Vector2Int.left;
            Vector2Int backward = -forward;
            
            // 범위 내 임의선택 가능한지
            bool canAccessAny = rangeData.skillType != SystemEnum.eSkillType.PhysicalAttack;
            
            // 임의선택 불가 시 block해줄 flag
            bool blocked = false;
            
            #region y = 0
            
            if (rangeData.Origin)
            {
                if(IsMaskMatch(grid, origin, maskType)) targetable.Add(origin);
                else unable.Add(origin);
            }
            
            for (int i = 1; i <= rangeData.Forward; i++)
            {
                Vector2Int pos = origin + forward * i;
                if (!grid.IsInBounds(pos) || !grid.IsPlatform(pos)) continue; // 범위 밖이거나 플랫폼이 아니면 무시
                if (!canAccessAny && blocked)
                {
                    // 물리공격이고 현재 막혀있는 상태면 불가 타일
                    unable.Add(origin);
                    continue;
                }
                
                // 범위 안, 플랫폼.
                if (IsMaskMatch(grid, pos, maskType))
                {
                    blocked = true;
                    targetable.Add(pos);
                }
                else // 캐릭터에 의해 차지되지 않았을 때
                {
                    
                    // 장애물일 때
                    if (grid.IsObstacle(pos))
                    {
                        blocked = true;
                    }

                    unable.Add(pos);
                }
            }
            
            blocked = false;
            for (int i = 1; i <= rangeData.Backward; i++)
            {
                Vector2Int pos =  origin - forward * i;
                if (!grid.IsInBounds(pos) || !grid.IsPlatform(pos)) continue;
                
                if (!canAccessAny && blocked)
                {
                    // 물리공격이고 현재 막혀있는 상태면 불가 타일
                    unable.Add(origin);
                    continue;
                }
                
                // 범위 안, 플랫폼.
                if (IsMaskMatch(grid, pos, maskType))
                {
                    blocked = true;
                    targetable.Add(pos);
                }
                else // 캐릭터에 의해 차지되지 않았을 때
                {
                    
                    // 장애물일 때
                    if (grid.IsObstacle(pos))
                    {
                        blocked = true;
                    }

                    unable.Add(pos);
                }
            }
            
            
            #endregion
            
            #region y = 1
            
            if (rangeData.Up)
            {
                Vector2Int pos = origin + Vector2Int.up;
                if (grid.IsInBounds(pos) && grid.IsPlatform(pos))
                {
                    if(IsMaskMatch(grid, pos, maskType)) targetable.Add(origin);
                    else unable.Add(origin);
                }
            }
            
            blocked = false;
            for (int i = 1; i <= rangeData.UpForward; i++)
            {
                Vector2Int pos = origin + forward * i + Vector2Int.up;
                if (!grid.IsInBounds(pos) || !grid.IsPlatform(pos)) continue;
                
                if (!canAccessAny && blocked)
                {
                    // 물리공격이고 현재 막혀있는 상태면 불가 타일
                    unable.Add(origin);
                    continue;
                }
                
                // 범위 안, 플랫폼.
                if (IsMaskMatch(grid, pos, maskType))
                {
                    blocked = true;
                    targetable.Add(pos);
                }
                else // 캐릭터에 의해 차지되지 않았을 때
                {
                    
                    // 장애물일 때
                    if (grid.IsObstacle(pos))
                    {
                        blocked = true;
                    }

                    unable.Add(pos);
                }
            }
            
            blocked = false;
            for (int i = 1; i <= rangeData.UpBackward; i++)
            {
                Vector2Int pos = origin - forward * i + Vector2Int.up;
                if (!grid.IsInBounds(pos) || !grid.IsPlatform(pos)) continue;
                
                if (!canAccessAny && blocked)
                {
                    // 물리공격이고 현재 막혀있는 상태면 불가 타일
                    unable.Add(origin);
                    continue;
                }
                
                // 범위 안, 플랫폼.
                if (IsMaskMatch(grid, pos, maskType))
                {
                    blocked = true;
                    targetable.Add(pos);
                }
                else // 캐릭터에 의해 차지되지 않았을 때
                {
                    
                    // 장애물일 때
                    if (grid.IsObstacle(pos))
                    {
                        blocked = true;
                    }

                    unable.Add(pos);
                }
            }
            #endregion
            
            #region y = -1
            
            
            
            if (rangeData.Down)
            {
                Vector2Int pos = origin + Vector2Int.down;
                
                if (grid.IsInBounds(pos) && grid.IsPlatform(pos))
                {
                    if(IsMaskMatch(grid, pos, maskType)) targetable.Add(origin);
                    else unable.Add(origin);
                }
            }
            
            blocked = false;
            for (int i = 1; i <= rangeData.DownForward; i++)
            {
                Vector2Int pos = origin + forward * i + Vector2Int.down;
                if (!grid.IsInBounds(pos) || !grid.IsPlatform(pos)) continue;
                
                if (!canAccessAny && blocked)
                {
                    // 물리공격이고 현재 막혀있는 상태면 불가 타일
                    unable.Add(origin);
                    continue;
                }
                
                // 범위 안, 플랫폼.
                if (IsMaskMatch(grid, pos, maskType))
                {
                    blocked = true;
                    targetable.Add(pos);
                }
                else // 캐릭터에 의해 차지되지 않았을 때
                {
                    
                    // 장애물일 때
                    if (grid.IsObstacle(pos))
                    {
                        blocked = true;
                    }

                    unable.Add(pos);
                }
            }
            
            blocked = false;
            for (int i = 1; i <= rangeData.DownBackward; i++)
            {
                Vector2Int pos = origin + forward * i - Vector2Int.down;
                if (!grid.IsInBounds(pos) || !grid.IsPlatform(pos)) continue;
                
                if (!canAccessAny && blocked)
                {
                    // 물리공격이고 현재 막혀있는 상태면 불가 타일
                    unable.Add(origin);
                    continue;
                }
                
                // 범위 안, 플랫폼.
                if (IsMaskMatch(grid, pos, maskType))
                {
                    blocked = true;
                    targetable.Add(pos);
                }
                else // 캐릭터에 의해 차지되지 않았을 때
                {
                    
                    // 장애물일 때
                    if (grid.IsObstacle(pos))
                    {
                        blocked = true;
                    }

                    unable.Add(pos);
                }
            }
            
            #endregion
            
            return new BattleActionPreviewData(targetable, unable);
        }
    }
}