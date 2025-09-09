using System;

namespace UIs.Runtime
{
    public interface IPresenter : IDisposable
    {
        void Hide();
        void OnBackRequested();
    }
}