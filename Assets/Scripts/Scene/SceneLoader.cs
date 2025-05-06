using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

namespace AngelBeat.Scene
{
    public static class SceneLoader
    {
        public static SystemEnum.eScene DestinationScene { get; private set; }
        public static Action InitCallback { get; private set; }

        public static void LoadSceneWithLoading(SystemEnum.eScene destination, Action sceneInitCallback=null)
        {
            DestinationScene = destination;
            InitCallback = sceneInitCallback;

            SceneManager.LoadScene(nameof(SystemEnum.eScene.LoadingScene));
        }

        public static void Clear()
        {
            DestinationScene = SystemEnum.eScene.None;
            InitCallback = null;
        }
    }
}