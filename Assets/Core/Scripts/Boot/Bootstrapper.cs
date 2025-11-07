using Core.Scripts.Foundation.Define;
using Core.Scripts.Foundation.SceneUtil;
using Core.Scripts.Managers;
using Cysharp.Threading.Tasks;
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
        
        [SerializeField] private AssetReferenceGameObject uiManagerReference;
        [SerializeField] private AssetReferenceGameObject mainCameraReference;
        [SerializeField] private AssetReferenceGameObject eventSystemReference;
        
        protected async void Start()
        {
            try
            {
                GameManager.Instance.GameState = SystemEnum.GameState.Loading;
                await GameReady.InitializeOnceAsync(); // 전체 매니저 초기화
                
                (GameObject ui, GameObject cam, GameObject es) = await UniTask.WhenAll(
                    ResourceManager.Instance.InstantiateAsync(uiManagerReference),
                    ResourceManager.Instance.InstantiateAsync(mainCameraReference),
                    ResourceManager.Instance.InstantiateAsync(eventSystemReference)
                );

                DontDestroyOnLoad(ui);
                DontDestroyOnLoad(cam);
                DontDestroyOnLoad(es);
                
                SceneLoader.LoadSceneWithLoading(nextScene); // nextScene 로드
            }
            catch (OperationCanceledException) { }
            catch (Exception e)
            {
                Debug.LogException(e);
            } 
        }
        
        
    }
}