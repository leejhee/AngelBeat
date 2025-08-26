using Core.UIAbstraction;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace UIs.Runtime
{
    public interface IViewFactory
    {
        UniTask<IPresenter> CreatePresenterAsync
            (string route, Transform parent, CancellationToken ct = default);
    }
}