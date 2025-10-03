using Cysharp.Threading.Tasks;
using System.Threading;
using UIs.Runtime;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GamePlay.Features.Battle.Scripts.UI
{
    public class GameOverView : MonoBehaviour, IView
    {
        [SerializeField] private Button toLobbyButton;
        public Button ToLobbyButton => toLobbyButton;
        // void Start()
        // {
        //     //StartCoroutine(QuitCountDown());
        // }
    
        // IEnumerator QuitCountDown()
        // {
        //     yield return new WaitForSeconds(3);
        //     Application.Quit();
        // }

        public GameObject Root { get; }
        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);

        public UniTask PlayEnterAsync(CancellationToken ct) => UniTask.CompletedTask;
        public UniTask PlayExitAsync(CancellationToken ct) => UniTask.CompletedTask;
    }

    public class GameOverPresenter : PresenterBase<GameOverView>
    {
        public GameOverPresenter(IView view) : base(view)
        {
        }

        public override UniTask EnterAction(CancellationToken token)
        {
            ViewEvents.Subscribe(
                act => View.ToLobbyButton.onClick.AddListener(new UnityAction(act)),
                act => View.ToLobbyButton.onClick.RemoveAllListeners(),
                ToLobby
            );
            
            return UniTask.CompletedTask;
        }

        private void ToLobby()
        {
            Debug.Log("로비로");
        }
    }
}
