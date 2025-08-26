using System.Threading;
using System.Threading.Tasks;

namespace Core.UIAbstraction
{
    public interface IPresenter
    {
        string Route { get; }
        bool IsVisible { get; }
        int Layer { get; }
        Task ShowAsync(CancellationToken ct=default);
        Task HideAsync(CancellationToken ct=default);
        void OnFocusGained();
        void OnFocusLost();
        bool OnBackRequested();
    }
}