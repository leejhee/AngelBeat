using Core.Scripts.Foundation.Define;
using Core.Scripts.Foundation.SceneUtil;
using GamePlay.Features.Battle.Scripts;
using GamePlay.Features.Explore.Scripts;

namespace GamePlay.Common.Scripts.Scene
{
    public static class GamePlaySceneUtil
    {
        public static void LoadBattleScene()
        {
            SceneLoader.LoadSceneWithLoading(SystemEnum.eScene.BattleTestScene, BattleSceneInitializer.InitializeAsync);
        }

        public static void LoadExploreScene()
        {
            SceneLoader.LoadSceneWithLoading(SystemEnum.eScene.ExploreScene, ExploreInitializer.InitializeAsync);
        }
    }
}