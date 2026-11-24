using UnityEngine;
using UnityEngine.Events;

namespace MilkMaker.Interaction
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class InteractionTarget : MonoBehaviour
    {
        [SerializeField]
        private UnityEvent _onInteract = new UnityEvent();

        public void Interact()
        {
            if (!isActiveAndEnabled)
                return;

            _onInteract.Invoke();
        }
    }
}