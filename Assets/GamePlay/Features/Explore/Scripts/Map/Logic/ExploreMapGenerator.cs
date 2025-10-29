using GamePlay.Features.Explore.Scripts.Map.Data;
using System;

namespace GamePlay.Features.Explore.Scripts.Map.Logic
{
    public static class ExploreMapGenerator
    {
        public static ExploreMapSkeleton BuildSkeleton(ExploreMapConfig cfg, int seed)
        {
            if (cfg == null) throw new ArgumentNullException(nameof(cfg));
            if (cfg.xCapacity <= 0 || cfg.yCapacity <= 0) 
                throw new ArgumentException("Invalid grid size");
            /*
            var builder = new MapBuilder(cfg, seed);
            builder.ComputeInteriorMask();          // 마름모 유효 영역 Floor, 나머지 None/Wall 후보
            builder.InitWalls();                    // 전부 Wall로 초기화
            builder.ChooseAnchors();                // Mask 내부에서 서로 먼 두 점 선정
            builder.CarveMainSpine();               // 앵커 사이 최단 경로만 Floor로 파기
            builder.GrowBranchesThin(
                minCoverage: 0.44f, maxCoverage: 0.52f,
                junctionChance: 0.12f,
                forwardBias: 0.75f,
                seedFromSpine: true,
                seedStep: 4,
                skipFromStart: 2, skipFromEnd: 2,
                maxSeedPerSide: 6,
                minLen: 4, maxLen: 12,
                straightLimit: 4, turnBonus: 0.20f
            );            
            builder.ConnectAllFloors();  
            //builder.FillEnclosedWallIslands();
            builder.FillTinyWallPockets();
            builder.SealBorderWalls(1);
            builder.ScatterSymbols();               // 심볼 배치(앵커/가중치 준수)
            builder.BasicValidate();                // 최소 검증
            */
            /*
            var builder = new DenseMapBuilder(cfg, seed);

            // 1. 타원형 내부 영역 계산
            builder.ComputeInteriorMask();

            // 2. 전체를 Wall로 초기화
            builder.InitWalls();

            // 3. 내부를 70% 확률로 Floor로 랜덤 채우기
            builder.RandomFillInterior(floorProbability: 0.70f);

            // 4. Cellular Automata 4회 반복
            // (이 과정에서 자연스럽게 클러스터링되고 균일해짐)
            builder.ApplyCellularAutomata(iterations: 4);

            // 5. Start/End 앵커 선택 (가장 먼 두 Floor 지점)
            builder.ChooseAnchors();

            // 6. 고립된 Floor 영역들을 연결
            builder.ConnectAllFloors();

            // 7. 경계 벽 봉인
            builder.SealBorderWalls(thickness: 1);

            // 8. 심볼 배치 (Start/End는 앵커에 배치됨)
            builder.ScatterSymbols();

            // 9. 최종 검증
            builder.BasicValidate();
            */
            /*
            var builder = new BSPMapBuilder(cfg, seed);

            // 1. 타원형 내부 영역 계산
            builder.ComputeInteriorMask();

            // 2. 전체를 Wall로 초기화
            builder.InitWalls();

            // 3. BSP로 방과 복도 생성
            builder.GenerateRoomsAndCorridors(
                minRoomSize: 5,        // 최소 방 크기
                maxDepth: 4,           // BSP 깊이 (4 = 최대 16개 방)
                roomFillRatio: 0.65f,  // 방이 리프 영역의 65% 차지
                corridorWidth: 1       // 복도 너비 (1-2칸)
            );

            // 4. 막다른 복도 정리 (선택사항)
            builder.PruneDeadEnds(iterations: 2);

            // 5. Start/End 앵커 선택 (가장 먼 두 방)
            builder.ChooseAnchors();

            // 6. 경계 벽 봉인
            builder.SealBorderWalls(thickness: 1);

            // 7. 심볼 배치
            builder.ScatterSymbols();

            // 8. 최종 검증
            builder.BasicValidate();*/
            /*
            var builder = new NodeMapBuilder(cfg, seed);

            // 1. 타원형 내부 영역 계산
            builder.ComputeInteriorMask();

            // 2. 전체를 Wall로 초기화
            builder.InitWalls();

            // 3. 노드 배치 (Poisson Disk Sampling)
            builder.PlaceNodes(
                targetCount: 12,   // 목표 노드 개수
                minDistance: 5     // 노드 간 최소 거리
            );

            // 4. MST로 노드 연결
            builder.ConnectNodesWithMST();

            // 5. 추가 연결 (루프 생성)
            builder.AddExtraConnections(count: 2);

            // 6. 노드를 Floor로 파기 (1-2칸 반경)
            builder.CarveNodes();

            // 7. 노드 간 복도 생성 (1칸 너비)
            builder.CarveCorridors();

            // 8. Start/End 앵커 선택 (가장 먼 두 노드)
            builder.ChooseAnchors();

            // 9. 경계 벽 봉인
            builder.SealBorderWalls(thickness: 1);

            // 10. 심볼 배치
            builder.ScatterSymbols();

            // 11. 최종 검증
            builder.BasicValidate();
            */
            /*
            var builder = new NodeMapBuilder(cfg, seed);
            
            // 1. 타원형 내부 영역 계산
            builder.ComputeInteriorMask();

            // 2. 전체를 Wall로 초기화
            builder.InitWalls();

            // 3. EndPoint와 Boss 배치 (서로 멀리)
            builder.PlaceEndAndBoss(minDistance: 8);

            // 4. Boss에서 역방향으로 트리 성장
            builder.GrowTreeFromBoss(
                targetDepth: 4,              // 최대 깊이 (Boss = 1, Start = 4)
                minBranchesPerNode: 2,       // 노드당 최소 분기 수
                maxBranchesPerNode: 4,       // 노드당 최대 분기 수
                minDistance: 4               // 노드 간 최소 거리
            );

            // 5. 가장 먼 leaf를 Start로 선택
            builder.ChooseStartFromLeaves();

            // 6. 노드를 Floor로 파기
            builder.CarveNodes();

            // 7. 부모-자식 관계를 따라 복도 생성
            builder.CarveCorridors();

            // 8. 경계 벽 봉인
            builder.SealBorderWalls(thickness: 1);

            // 9. 심볼 배치 (Critical Path 제외)
            builder.ScatterSymbols();

            // 10. 최종 검증
            builder.BasicValidate();
*/
            
            var builder = new NodeMapBuilder(cfg, seed);
            
            // 1. 타원형 내부 영역 계산
            builder.ComputeInteriorMask();

            // 2. 전체를 Wall로 초기화
            builder.InitWalls();

            // 3. Boss와 End를 인접하게 배치
            builder.PlaceEndAndBossAdjacent(adjacentDistance: 1);

            // 4. Boss에서 Start 방향으로 메인 스파인 생성
            builder.CreateMainSpineFromBoss(
                minLength: 9,
                maxLength: 13,
                minSpacing: 5,
                maxSpacing: 7
            );

            // 5. 메인 스파인에서 사이드 브랜치 생성 (선택적)
            builder.AddSideBranches(
                branchProbability: 0.25f,
                minBranchLength: 1,
                maxBranchLength: 3
            );

            // 6. 가장 먼 스파인 노드를 Start로 선택
            builder.ChooseStartFromFurthest();

            // 7. 노드를 Floor로 파기
            builder.CarveNodes();

            // 8. 노드 간 복도 생성
            builder.CarveCorridors();
            //builder.RemoveDiagonalTouches();
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

            return builder.ToSkeleton();
        }
        
        public static ExploreMapSkeleton BuildSkeletonCustom(
            ExploreMapConfig cfg,
            int seed,
            float initialFloorDensity = 0.70f,
            int caIterations = 4)
        {
            if (cfg == null) throw new ArgumentNullException(nameof(cfg));
            if (cfg.xCapacity <= 0 || cfg.yCapacity <= 0)
                throw new ArgumentException("Invalid grid size");

            var builder = new DenseMapBuilder(cfg, seed);

            builder.ComputeInteriorMask();
            builder.InitWalls();
            builder.RandomFillInterior(floorProbability: initialFloorDensity);
            builder.ApplyCellularAutomata(iterations: caIterations);
            builder.ChooseAnchors();
            builder.ConnectAllFloors();
            builder.SealBorderWalls(thickness: 1);
            builder.ScatterSymbols();
            builder.BasicValidate();

            return builder.ToSkeleton();
        }
    }
}