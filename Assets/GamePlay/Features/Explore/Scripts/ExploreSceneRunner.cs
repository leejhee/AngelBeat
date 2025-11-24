using Cysharp.Threading.Tasks;
using UIs.Runtime;

namespace GamePlay.Features.Explore.Scripts
{
    public static class ExploreSceneRunner
    {
        private static bool scheduled;

        public static void RunAfterLoading()
        {
            if (scheduled) return;
            scheduled = true;
            
            
        }
    }
}