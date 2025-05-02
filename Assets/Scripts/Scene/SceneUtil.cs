using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

namespace AngelBeat.Scene
{
    public static class SceneUtil
    {
        public static async Task LoadSceneAdditiveAsync(string sceneName)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            if (asyncLoad == null)
            {
                Debug.LogError("SceneUtil.LoadSceneAdditiveAsync: asyncLoad is null");
                return;
            }

            while (asyncLoad.isDone == false)
            {
                await Task.Yield();
            }
            
            Debug.Log("SceneUtil.LoadSceneAdditiveAsync: asyncLoad is done");
        }
        
        public static async Task LoadSceneAsync(string sceneName)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            if (asyncLoad == null)
            {
                Debug.LogError("SceneUtil.LoadSceneAdditiveAsync: asyncLoad is null");
                return;
            }

            while (asyncLoad.isDone == false)
            {
                await Task.Yield();
            }
            
            Debug.Log("SceneUtil.LoadSceneAdditiveAsync: asyncLoad is done");
        }
        
        public static async Task UnloadSceneAsync(string sceneName)
        {
            AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(sceneName);

            if (asyncUnload == null)
            {
                Debug.LogError($"씬 언로드 실패: {sceneName}");
                return;
            }

            while (!asyncUnload.isDone)
            {
                await Task.Yield();
            }

            Debug.Log($"씬 {sceneName} 언로드 완료");
        }
    }
}