using Cysharp.Threading.Tasks;
using GamePlay.Contracts;
using System.Threading;
using UnityEngine;

namespace GamePlay.Features.Scripts.Interaction
{
    /// <summary>
    /// 상호작용 가능한 클래스(임시)
    /// 해당 상속 불필요 시 바로 구체 클래스로 옮기고 폐기 예정
    /// </summary>
    public abstract class Interactor : MonoBehaviour, IInteractable
    {
        public string Prompt { get; private set; }
        public Transform ParentTransform { get; private set; }

        public abstract bool Interactable(GameObject targetActor);
        public abstract UniTask Interact(GameObject targetActor, CancellationToken ct);
    }
}