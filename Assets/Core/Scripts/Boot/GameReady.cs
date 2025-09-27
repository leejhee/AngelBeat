using Core.Scripts.Managers;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Scripts.Boot
{
    public static class GameReady
    {
        private static bool isReady;

        public static async UniTask InitializeOnceAsync()
        {
            if (isReady) return;
            await ResourceManager.Instance.InitAsync();
            await DataManager.Instance.InitAsync();
            
            //산하에 SingletonObject<T> 상속받는 매니저들 동기 초기화. Managers 어셈블리 내의 매니저들만 Init하자.
            SaveLoadManager.Instance.Init();
            SoundManager.Instance.Init();
            //InputManager.Instance.Init();
            
            isReady = true;
            
            Debug.Log("All Managers Initialized to Run");
        }
    }
}