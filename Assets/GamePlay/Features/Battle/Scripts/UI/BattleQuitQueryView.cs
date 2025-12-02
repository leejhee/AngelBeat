using Cysharp.Threading.Tasks;
using System.Threading;
using UIs.Runtime;
using UnityEngine;

namespace GamePlay.Features.Battle.Scripts.UI
{
    public class BattleQuitQueryView : MonoBehaviour, IView
    {
        public GameObject Root { get; }
        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);

        public UniTask PlayEnterAsync(CancellationToken ct) => UniTask.CompletedTask;
        public UniTask PlayExitAsync(CancellationToken ct) => UniTask.CompletedTask;

        public void OnCancelButtonClick()
        {
            
        }
        
    }
}