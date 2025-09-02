using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace GamePlay.Contracts
{
    public interface IInteractable
    {
        string Prompt { get; }
        Transform ParentTransform { get; }
        //int Priority { get; } // 다수 상호작용 시의 우선순위인데, 현재 필요없어보임.
        bool Interactable(GameObject targetActor);
        UniTask Interact (GameObject targetActor, CancellationToken ct);
    }
}