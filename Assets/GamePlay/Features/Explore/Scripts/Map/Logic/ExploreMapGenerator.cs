using Cysharp.Threading.Tasks;
using GamePlay.Features.Explore.Scripts.Map.Data;
using System;

namespace GamePlay.Features.Explore.Scripts.Map.Logic
{
    public static class ExploreMapGenerator
    {
        public static async UniTask<ExploreMapSkeleton> BuildSkeleton(ExploreMapConfig cfg, ulong seed)
        {
            if (!cfg) throw new ArgumentNullException(nameof(cfg));
            if (cfg.useBakedSkeleton && cfg.bakedMapAsset && cfg.bakedMapAsset.skeleton != null)
            {
                return cfg.bakedMapAsset.skeleton;
            }
            
            if (cfg.xCapacity <= 0 || cfg.yCapacity <= 0) 
                throw new ArgumentException("Invalid grid size");

            await UniTask.SwitchToThreadPool();
            
            var builder = new NodeMapBuilder(cfg, seed);
            
            // 1. 타원형 내부 영역 계산
            builder.ComputeInteriorMask();

            // 2. 전체를 Wall로 초기화
            builder.InitWalls();

            // 3. Boss와 End를 인접하게 배치
            builder.PlaceEndAndBossAdjacent(adjacentDistance: 1);

            // 4. Boss에서 Start 방향으로 메인 스파인 생성
            builder.CreateMainSpineFromBoss(
                minLength: 14,
                maxLength: 20,
                minSpacing: 7,
                maxSpacing: 10
            );

            // 5. 메인 스파인에서 사이드 브랜치 생성 (선택적)
            builder.AddSideBranches(
                branchProbability: 0.12f,
                minBranchLength: 2,
                maxBranchLength: 6
            );

            // 6. 가장 먼 스파인 노드를 Start로 선택
            builder.ChooseStartFromFurthest();

            // 7. 노드를 Floor로 파기
            builder.CarveNodes();

            // 8. 노드 간 복도 생성
            builder.CarveCorridors();
            
            builder.EnsureStartBossConnectivity();
            builder.EnforceEndLockedByBoss();
            builder.RemoveDiagonalTouches();
            builder.CullFloorsNotReachableFromBoss();
            
            // 9. 경계 벽 봉인
            builder.SealBorderWalls(thickness: 1);
            
            // 10. 심볼 배치
            builder.ScatterSymbols();

            // 11. 최종 검증
            builder.BasicValidate();
            
            await UniTask.SwitchToMainThread();
            
            return builder.ToSkeleton();
        }
        
    }
}