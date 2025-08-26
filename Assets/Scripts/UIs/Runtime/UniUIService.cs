using Core.UIAbstraction;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace UIs.Runtime
{
    public sealed class UniUIService : MonoBehaviour, IUIService, IUniUIService 
    {
        [SerializeField] UINavigator navigator;
        public UniTask OpenUniAsync(string r, object vm=null, CancellationToken ct=default)
            => navigator.PushPresenterAsync(r, vm, ct);
        public UniTask<bool> BackUniAsync(CancellationToken ct=default)
            => navigator.BackAsync(ct);
        public UniTask CloseTopUniAsync(CancellationToken ct=default)
            => navigator.PopAsync(ct);
        public UniTask CloseAllUniAsync(CancellationToken ct=default)
            => navigator.CloseAllAsync(ct);
        
        public Task OpenAsync(string r, object vm=null, CancellationToken ct=default)
            => OpenUniAsync(r, vm, ct).AsTask();
        public Task<bool> BackAsync(CancellationToken ct=default)
            => BackUniAsync(ct).AsTask();
        public Task CloseTopAsync(CancellationToken ct=default)
            => CloseTopUniAsync(ct).AsTask();
        public Task CloseAllAsync(CancellationToken ct=default)
            => CloseAllUniAsync(ct).AsTask();
    }
}