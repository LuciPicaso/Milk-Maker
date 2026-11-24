using UnityEngine;

namespace MilkMaker.Items
{
    [DisallowMultipleComponent]
    public sealed class Bucket : MonoBehaviour
    {
        [SerializeField] private ItemHolder _holder;
        [SerializeField] private Collider _interactionCollider;

        public BucketSocket CurrentSocket { get; private set; }

        public bool IsHeld { get; private set; }

        public void Interact()
        {
            if (_holder != null)
                _holder.TryPickUp(this);
        }

        internal void AttachToHand(Transform anchor)
        {
            CurrentSocket = null;
            IsHeld = true;

            Attach(anchor);

            if (_interactionCollider != null)
                _interactionCollider.enabled = false;
        }

        internal void AttachToSocket(
            BucketSocket socket,
            Transform anchor)
        {
            CurrentSocket = socket;
            IsHeld = false;

            Attach(anchor);

            if (_interactionCollider != null)
                _interactionCollider.enabled = true;
        }

        private void Attach(Transform anchor)
        {
            transform.SetParent(anchor, false);

            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
    }
}