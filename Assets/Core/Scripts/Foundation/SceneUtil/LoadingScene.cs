using Cysharp.Threading.Tasks;
using System;
using System.Collections;
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
        private IEnumerator Start()
        {
            string destination = SceneLoader.DestinationScene.ToString();
            AsyncOperation op = SceneManager.LoadSceneAsync(destination, LoadSceneMode.Single);
            op.allowSceneActivation = false;

            while (op.progress < loadingBoundary)
            {
                float p = Mathf.Clamp01(op.progress / loadingBoundary);
                progressBar.fillAmount = p * loadingBoundary;
                yield return null;
            }

            CancellationTokenSource cts = new();
            if (SceneLoader.InitCallbackAsync != null)
            {
                float post = 0f;
                Progress<float> progress = new(p => {
                    progressBar.fillAmount = loadingBoundary + (0.1f * Mathf.Clamp01(p));
                    post = p;
                });
                yield return SceneLoader.InitCallbackAsync(cts.Token).ToCoroutine();
            }
            SceneLoader.Clear();
            
            //float elapsed = 0f;
            //while (elapsed < fakeLoadingTime)
            //{
            //    elapsed += Time.deltaTime;
            //    float t = Mathf.Clamp01(elapsed / fakeLoadingTime);
            //    progressBar.fillAmount = 0.9f + 0.1f * t;
            //    yield return null;
            //}

            op.allowSceneActivation = true;
            yield return op;
        }
    }
}