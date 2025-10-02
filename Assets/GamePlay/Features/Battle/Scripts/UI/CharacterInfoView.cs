using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UIs.Runtime;
using UnityEngine;

public class CharacterInfoView : MonoBehaviour, IView
{

    public GameObject Root { get; }
    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);

    public UniTask PlayEnterAsync(CancellationToken ct) => UniTask.CompletedTask;
    public UniTask PlayExitAsync(CancellationToken ct) => UniTask.CompletedTask;
}

public class CharacterInfoPresenter : PresenterBase<CharacterInfoView>
{
    public CharacterInfoPresenter(IView view) : base(view) { }

    public override UniTask EnterAction(CancellationToken token)
    {
        
        
        return UniTask.CompletedTask;
    }
}