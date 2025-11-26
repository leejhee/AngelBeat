using Core.Scripts.Foundation.Define;
using Core.Scripts.Foundation.SceneUtil;
using Core.Scripts.Foundation.Utils;
using Core.Scripts.Managers;
using Cysharp.Threading.Tasks;
using GamePlay.Features.Battle.Scripts.BattleMap;
using GamePlay.Features.Battle.Scripts.BattleTurn;
using GamePlay.Features.Battle.Scripts.Tutorial;
using System;
using System.Threading;
using UIs.Runtime;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GamePlay.Features.Battle.Scripts
{
    public static class BattleSceneInitializer
    {
        public static async UniTask InitializeAsync(CancellationToken ct, IProgress<float> progress)
        {
            try 
            {
                Debug.Log("———————————————Battle Initializing...———————————————");
                IBattleSceneSource src = ResolveSource();
                Debug.Log($"[Battle Initializer] Source = {src.GetType().Name}, Dungeon={src.Dungeon}, Stage={src.StageName}");
                progress?.Report(0.05f);
                
                Debug.Log("[Game Manager] GameState Change : Battle");
                GameManager.Instance.GameState = SystemEnum.GameState.Battle;
                progress?.Report(0.1f);
            
                Debug.Log("[Battle Initializer] Camera Setting for Battle");
                Camera mainCamera = Camera.main;
                Camera backCamera = GameObject.Find("BackgroundCamera").GetComponent<Camera>(); //그냥 널가드 안하고 할거임.
                CameraUtil.TryStackOverlay(backCamera, mainCamera);
                
                backCamera.clearFlags = CameraClearFlags.SolidColor;
                backCamera.backgroundColor = Color.black;
                
                progress?.Report(0.2f);
                
                Debug.Log("[Battle Initializer] 맵 로딩 시작...");
                BattleController controller = Object.FindObjectOfType<BattleController>();
                if (controller == null)
                {
                    throw new Exception("BattleController를 씬에서 찾을 수 없습니다!");
                }

                BattleFieldDB db = await ResourceManager.Instance.LoadAsync <BattleFieldDB>("BattleFieldDB", ct);
                StageLoader loader = new(src, db);
                StageField stage = await loader.InstantiateBattleFieldAsync(src.StageName, null, ct);
                
                BattleCameraInput input = Object.FindFirstObjectByType<BattleCameraInput>(FindObjectsInactive.Exclude);
                if (input)
                {
                    input.Bind(stage, controller.CameraDriver);
                    input.enableDuringTurn = false;
                }
                
                Debug.Log("[Battle Initializer] 맵 로딩 완료!");
                progress?.Report(0.4f);
            
                Debug.Log("[Battle Initializer] 그리드 초기화...");
                BattleStageGrid stageGrid = stage.GetComponent<BattleStageGrid>() 
                                            ?? stage.gameObject.AddComponent<BattleStageGrid>();
                stageGrid.InitGrid(stage);
                progress?.Report(0.5f);
            
                Debug.Log("[Battle Initializer] 유닛 스폰 시작...");
                await stage.SpawnAllUnits(src.PlayerParty);
                stageGrid.RebuildCharacterPositions();
                
                Debug.Log("[Battle Initializer] 유닛 스폰 완료!");
                progress?.Report(0.7f);
                
                Debug.Log("[Battle Initializer] 턴 관리 및 이벤트 주입 중...");
                TurnController turnManager = new(); 
                
                BattleTutorialDirector tutorialDirector = Object.FindFirstObjectByType<BattleTutorialDirector>(FindObjectsInactive.Exclude);
                if (tutorialDirector != null)
                {
                    tutorialDirector.Init(turnManager, controller);
                }
                else
                {
                    Debug.Log("[BattleSceneInitializer] BattleTutorialDirector not found in scene.");
                }
                
                Debug.Log("[Battle Initializer] 턴 관리 및 이벤트 주입 완료!");
                progress?.Report(0.8f);
            
                controller.Initialize(stage, turnManager, src.PlayerParty, src.ReturningScene);
                progress?.Report(0.9f);
                
                Debug.Log("[Battle Initializer] UI 초기화...");
                await UIManager.Instance.ShowViewAsync(ViewID.BattleSceneView);
                
                var bgm = await ResourceManager.Instance.LoadAsync<AudioClip>(BattleController.Instance.bgmRef);
                SoundManager.Instance.Play(bgm, SystemEnum.Sound.Bgm, pitch: 1f);
                
                BattleSceneRunner.RunAfterLoading(stage, turnManager);
                progress?.Report(1.0f);
                
                Debug.Log("———————————————Battle Initialization Complete———————————————");
            }
            catch (Exception e)
            {
                Debug.LogError("[BattleInit] 초기화 중 에러 발생");
                Debug.LogException(e);
                throw;
            }
        }

        private static IBattleSceneSource ResolveSource()
        {
            if (SceneLoader.SceneArgs is IBattleSceneSource args) return args;

            bool hasParty = BattlePayload.Instance != null && BattlePayload.Instance.PlayerParty != null;
            bool hasStage = BattlePayload.Instance != null && BattlePayload.Instance.StageNames.Count > 0;
            if (hasStage && hasParty)
                return new BattlePayloadSource();
            
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning("[Battle Initializer] No args/payload provided. Using DebugMockSource.Default.");
            return DebugMockSource.Default();
#else
            return DebugMockSource.Default();
            //throw new InvalidOperationException("[Battle Initializer] No battle args/payload provided.");
            
#endif
        }
        
    }
}