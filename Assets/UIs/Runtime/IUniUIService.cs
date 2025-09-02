using Cysharp.Threading.Tasks;
using System.Threading;

namespace UIs.Runtime
{
    public interface IUniUIService
    {
        UniTask OpenUniAsync(string route, object vm = null, CancellationToken ct = default);
        UniTask<bool> BackUniAsync(CancellationToken ct = default);
        UniTask CloseTopUniAsync(CancellationToken ct = default);
        UniTask CloseAllUniAsync(CancellationToken ct = default);
    }
}