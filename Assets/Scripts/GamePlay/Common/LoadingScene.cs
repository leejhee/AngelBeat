using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Scene
{
    public class LoadingScene : MonoBehaviour
    {
        [SerializeField] private Image progressBar;
        [SerializeField] private float fakeLoadingTime = 1f;
        private const float LoadingBoundary = 0.9f;
        private IEnumerator Start()
        {
            string destination = SceneLoader.DestinationScene.ToString();
            AsyncOperation op = SceneManager.LoadSceneAsync(destination, LoadSceneMode.Single);
            op.allowSceneActivation = false;

            while (op.progress < LoadingBoundary)
            {
                float p = Mathf.Clamp01(op.progress / LoadingBoundary);
                progressBar.fillAmount = p * LoadingBoundary;
                yield return null;
            }
            
            // TODO : Unitask 도입할 경우 이것도 비동기로 처리하기.
            SceneLoader.InitCallback?.Invoke();
            SceneLoader.Clear();
            
            float elapsed = 0f;
            while (elapsed < fakeLoadingTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fakeLoadingTime);
                progressBar.fillAmount = 0.9f + 0.1f * t;
                yield return null;
            }

            op.allowSceneActivation = true;
            yield return op;
        }
    }
}