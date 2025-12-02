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

        public UniTask PlayEnterAsync(CancellationToken ct)
        {
            throw new System.NotImplementedException();
        }

        public UniTask PlayExitAsync(CancellationToken ct)
        {
            throw new System.NotImplementedException();
        }

        public void OnCancelButtonClick()
        {
            
        }
        
    }
}