using Cysharp.Threading.Tasks;
using System.Threading;

namespace UIs.Runtime
{
    public interface IUniPresenter
    {
        UniTask ShowUniAsync(CancellationToken ct = default);
        UniTask HideUniAsync(CancellationToken ct = default);
    }
}