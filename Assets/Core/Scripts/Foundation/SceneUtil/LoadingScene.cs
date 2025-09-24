using Core.Scripts.Foundation.Define;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Core.Scripts.Foundation.SceneUtil
{
    public class LoadingScene : MonoBehaviour
    {
        [SerializeField] private Image progressBar;
        [SerializeField] private float fakeLoadingTime;
        [SerializeField] private float loadingBoundary = 0.5f;
        
        private CancellationTokenSource _cts;
        private async void Start()
        {
            var ct = this.GetCancellationTokenOnDestroy();
            
            #region Validating Destination
            string destination = SceneLoader.DestinationScene.ToString();
            if (string.IsNullOrEmpty(destination) || destination == nameof(SystemEnum.eScene.None))
            {
                Debug.LogError("Invalid Destination");
            }
            
            #endregion
            
            #region Scene Loading
            if(progressBar) progressBar.fillAmount = 0; 
            AsyncOperation op = SceneManager.LoadSceneAsync(destination, LoadSceneMode.Single);
            op.allowSceneActivation = false;

            while (op.progress < loadingBoundary)
            {
                if(progressBar)
                    progressBar.fillAmount = Mathf.Clamp01(op.progress / loadingBoundary) * loadingBoundary;
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }
            #endregion
            
            #region Loading Pipeline - After Scene Loading
            _cts = new CancellationTokenSource();
            try
            {
                if (SceneLoader.InitCallbackAsync != null)
                {
                    var progress = new Progress<float>(p => {
                        if(progressBar)
                            progressBar.fillAmount = loadingBoundary + ((1 - loadingBoundary) * Mathf.Clamp01(p));
                    });
                
                    await SceneLoader.InitCallbackAsync(_cts.Token, progress);
                }
                else
                {
                    if (progressBar) progressBar.fillAmount = 1f;
                    await UniTask.Yield(PlayerLoopTiming.Update, ct);
                }
            }
            catch (OperationCanceledException){}
            catch (Exception e)
            {
                Debug.LogException(e, this);
            }
            finally
            {
                SceneLoader.Clear(); 
            }
            
            #endregion
                
            op.allowSceneActivation = true;
            await op.ToUniTask();
        }

        private void OnDestroy()
        {
            _cts?.Dispose();
            _cts = null;
        }
    }
}