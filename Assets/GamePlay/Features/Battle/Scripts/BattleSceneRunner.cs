using Cysharp.Threading.Tasks;
using GamePlay.Features.Battle.Scripts.BattleMap;
using GamePlay.Features.Battle.Scripts.BattleTurn;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GamePlay.Features.Battle.Scripts
{
    public static class BattleSceneRunner
    {
        private static bool scheduled;

        public static void RunAfterLoading(StageField stage, TurnController turn)
        {
            if (scheduled) return;
            scheduled = true;

            UniTask.Void(async () =>
            {
                // 로딩씬 완전 언로드까지 대기
                await UniTask.WaitUntil(() =>
                {
                    UnityEngine.SceneManagement.Scene s = SceneManager.GetSceneByName("LoadingScene");
                    return !s.IsValid() || !s.isLoaded;
                });

                // 전장 전체 인트로
                var driver = Object.FindFirstObjectByType<BattleCameraDriver>();
                if (driver && stage)
                    await driver.ShowStageIntro(stage, paddingWorld:1.0f, fadeSeconds:0.8f);

                // 첫 턴 시작
                if (turn != null)
                    await turn.ChangeTurn();

                scheduled = false; // 한 번 끝났으면 해제(필요시 유지)
            });
        }
    }
}