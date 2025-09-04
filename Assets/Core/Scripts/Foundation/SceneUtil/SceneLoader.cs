using Core.Scripts.Foundation.Define;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine.SceneManagement;

namespace Core.Scripts.Foundation.SceneUtil
{
    public static class SceneLoader
    {
        public static SystemEnum.eScene DestinationScene { get; private set; }
        public static Func<CancellationToken, UniTask> InitCallbackAsync { get; private set; } // 파이프라인을 여기에 쌓을 것
        public static void LoadSceneWithLoading(SystemEnum.eScene destination, Func<CancellationToken, UniTask> sceneInitCallback=null)
        {
            DestinationScene = destination;
            InitCallbackAsync = sceneInitCallback;
            SceneManager.LoadScene(nameof(SystemEnum.eScene.LoadingScene));
        }

        public static void Clear()
        {
            DestinationScene = SystemEnum.eScene.None;
            //InitCallback = null;
            InitCallbackAsync = null;
        }
    }
}