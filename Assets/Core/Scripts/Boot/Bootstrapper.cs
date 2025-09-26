using Core.Scripts.Foundation.Define;
using Core.Scripts.Foundation.SceneUtil;
using Core.Scripts.Managers;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Core.Scripts.Boot
{
    /// <summary>
    /// 앱 시작 시 최초로 부트해주는 역할.
    /// </summary>
    public class Bootstrapper : MonoBehaviour
    {
        [SerializeField] private SystemEnum.eScene nextScene = SystemEnum.eScene.LobbyScene;

        [SerializeField] private AssetReferenceGameObject UIManagerReference;
        protected async void Start()
        {
            try
            {
                GameManager.Instance.GameState = SystemEnum.GameState.Loading;
                await GameReady.InitializeOnceAsync(); // 전체 매니저 초기화
                await ResourceManager.Instance.InstantiateAsync(UIManagerReference);
                SceneLoader.LoadSceneWithLoading(nextScene);
            }
            catch (OperationCanceledException) { }
            catch (Exception e)
            {
                Debug.LogException(e);
            } 
        }
        
    }
}