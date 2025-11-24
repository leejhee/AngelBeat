using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UIs.Runtime;

namespace GamePlay.Features.Explore.Scripts
{
    public static class ExploreInitializer
    {
        public static async UniTask InitializeAsync(CancellationToken ct, IProgress<float> progress)
        {
            await NovelManager.PlayScriptAndWait("1", ct);
            await NovelManager.PlayScriptAndWait("2", ct);
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