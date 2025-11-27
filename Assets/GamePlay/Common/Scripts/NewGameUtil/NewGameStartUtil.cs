using Core.Scripts.Foundation.Define;
using Cysharp.Threading.Tasks;
using GamePlay.Common.Scripts.Entities.Character;
using GamePlay.Common.Scripts.Novel;
using GamePlay.Common.Scripts.Scene;
using GamePlay.Features.Explore.Scripts;

namespace GamePlay.Common.Scripts.NewGameUtil
{
    public static class NewGameStartUtil
    {
        public static async UniTask StartNewGame()
        {
            Party newParty = new();
            newParty.InitParty();
            ExploreSession.Instance.SetNewExplore(SystemEnum.Dungeon.TUTORIAL, 1, newParty);
            await NovelDomainPlayer.PlayNovelScript("1");
            GamePlaySceneUtil.LoadExploreScene();
        }
    }
}