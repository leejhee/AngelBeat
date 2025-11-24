using Core.Scripts.Foundation.Define;
using Core.Scripts.Managers;
using Cysharp.Threading.Tasks;
using GamePlay.Common.Scripts.Entities.Character;
using System;
using System.Threading;
using UIs.Runtime;

namespace GamePlay.Features.Explore.Scripts
{
    public static class ExploreInitializer
    {
        public static async UniTask InitializeAsync(CancellationToken ct, IProgress<float> progress)
        {
            GameManager.Instance.GameState = SystemEnum.GameState.Explore;
            Party playerParty = new Party();
            ExploreManager.Instance.playerParty = playerParty;
            
            await NovelManager.PlayScriptAndWait("1", ct);
            //await NovelManager.PlayScriptAndWait("2", ct);
            progress?.Report(0.05f);

            await ExploreManager.Instance.InitializeForSceneAsync();
            progress?.Report(0.5f);
            
            await UIManager.Instance.ShowViewAsync(ViewID.ExploreSceneView);
            progress?.Report(0.8f);
            
            ExploreSceneRunner.RunAfterLoading();
            progress?.Report(1.0f);
        }
    }
}