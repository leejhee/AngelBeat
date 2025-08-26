using System.Threading;
using System.Threading.Tasks;

namespace Core.UIAbstraction
{
    public interface IUIService
    {
        Task OpenAsync(string route, object vm = null, CancellationToken ct = default);
        Task<bool> BackAsync(CancellationToken ct = default);
        Task CloseTopAsync(CancellationToken ct = default);
        Task CloseAllAsync(CancellationToken ct = default);
    }
}