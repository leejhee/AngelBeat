using Cysharp.Threading.Tasks;
using GamePlay.Contracts;
using System.Threading;
using UnityEngine;

namespace GamePlay.Features.Scripts.Interaction
{
    // 상호작용 필요한 entities의 추상(Battle과는 달라야 함.)
    // ui가 일단 나오도록 하는게 좋겠음.
    
    
    /// <summary>
    /// 상호작용 가능한 클래스(임시)
    /// 해당 상속 불필요 시 바로 구체 클래스로 옮기고 폐기 예정
    /// </summary>
    public abstract class InteractableBase : MonoBehaviour, IInteractable
    {
        [Header("UI Options")]
        [SerializeField] private string prompt;
        [SerializeField] private Transform parentTransform;
        [SerializeField] private bool isGuidePop;
        [SerializeField] private bool used; // 이미 끝난 상호작용인지.
        
        public virtual string Prompt => prompt;
        public virtual Transform ParentTransform => parentTransform;

        public abstract bool Interactable(GameObject targetActor);
        public abstract UniTask Interact(GameObject targetActor, CancellationToken ct);
    }
}