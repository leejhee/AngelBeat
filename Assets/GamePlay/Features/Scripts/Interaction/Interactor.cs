using Cysharp.Threading.Tasks;
using GamePlay.Contracts.Interaction;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace GamePlay.Features.Scripts.Interaction
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class Interactor : MonoBehaviour, IInteractor
    {
        public Transform Transform => transform;
        public CancellationToken LifeTimeToken => this.GetCancellationTokenOnDestroy();

        public async UniTask TryInteract(IInteractable target, CancellationToken ext)
        {
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(LifeTimeToken, ext);
            await target.Interact(this, linked.Token);
        }
        
    }
}