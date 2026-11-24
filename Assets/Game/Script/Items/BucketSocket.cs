using UnityEngine;

namespace MilkMaker.Items
{
    [DisallowMultipleComponent]
    public sealed class BucketSocket : MonoBehaviour
    {
        [SerializeField] private ItemHolder _holder;
        [SerializeField] private Transform _anchor;

        public Bucket OccupiedBucket { get; private set; }

        public bool IsOccupied =>
            OccupiedBucket != null;

        public void Interact()
        {
            if (_holder == null)
                return;

            if (IsOccupied)
            {
                _holder.TryPickUp(OccupiedBucket);
                return;
            }

            _holder.TryPlace(this);
        }

        internal bool TryOccupy(Bucket bucket)
        {
            if (_anchor == null ||
                bucket == null ||
                IsOccupied ||
                !bucket.IsHeld)
            {
                return false;
            }

            OccupiedBucket = bucket;

            bucket.AttachToSocket(this, _anchor);

            return true;
        }

        internal bool TryRelease(Bucket bucket)
        {
            if (bucket == null ||
                OccupiedBucket != bucket)
            {
                return false;
            }

            OccupiedBucket = null;

            return true;
        }
    }
}