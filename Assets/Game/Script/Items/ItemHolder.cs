using UnityEngine;
using UnityEngine.Events;

namespace MilkMaker.Items
{
    [DisallowMultipleComponent]
    public sealed class ItemHolder : MonoBehaviour
    {
        [SerializeField] private Transform _holdAnchor;

        [Header("Presentation")]
        [SerializeField] private UnityEvent _onPickedUp = new();
        [SerializeField] private UnityEvent _onPlaced = new();

        public Bucket HeldBucket { get; private set; }

        public bool HasItem => HeldBucket != null;

        public bool TryPickUp(Bucket bucket)
        {
            if (_holdAnchor == null ||
                bucket == null ||
                HasItem ||
                bucket.IsHeld)
            {
                return false;
            }

            BucketSocket socket = bucket.CurrentSocket;

            if (socket != null &&
                !socket.TryRelease(bucket))
            {
                return false;
            }

            HeldBucket = bucket;

            bucket.AttachToHand(_holdAnchor);

            _onPickedUp.Invoke();

            return true;
        }

        public bool TryPlace(BucketSocket socket)
        {
            if (!HasItem || socket == null)
                return false;

            if (!socket.TryOccupy(HeldBucket))
                return false;

            HeldBucket = null;

            _onPlaced.Invoke();

            return true;
        }
    }
}