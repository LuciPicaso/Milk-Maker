using UnityEngine;
using System;
using MilkMaker.Upgrades;

namespace MilkMaker.Items
{
    [DisallowMultipleComponent]
    public sealed class Bucket : MonoBehaviour
    {
        public UpgradeService Upgrades => _upgrades;
        
#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _debug;
#endif
        [Header("Milk Storage")]
        [SerializeField] private double _capacity = 10d;
        [SerializeField] private UpgradeService _upgrades;

        public double MilkAmount { get; private set; }

        public double Capacity =>
            _capacity * (
                _upgrades != null
                    ? _upgrades.BucketCapacityMultiplier
                    : 1d);

        public double RemainingCapacity =>
            Math.Max(0d, Capacity - MilkAmount);

        public bool IsFull => RemainingCapacity <= 0d;

        public bool IsEmpty => MilkAmount <= 0d;

        public float Fill01 =>
            Capacity > 0d
                ? Mathf.Clamp01((float)(MilkAmount / Capacity))
                : 0f;

        public event Action<double> OnMilkChanged;

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

        public double AddMilk(double amount)
        {
            if (double.IsNaN(amount) ||
                double.IsInfinity(amount) ||
                amount <= 0d ||
                double.IsNaN(Capacity) ||
                double.IsInfinity(Capacity) ||
                Capacity <= 0d ||
                IsFull)
            {
                return 0d;
            }

            double previousAmount = MilkAmount;

            MilkAmount = Math.Min(
                Capacity,
                MilkAmount + amount);

            double accepted = MilkAmount - previousAmount;

            if (accepted <= 0d)
                return 0d;

            OnMilkChanged?.Invoke(MilkAmount);

            return accepted;
        }

        internal double DrainMilk()
        {
            if (IsEmpty)
                return 0d;

            double amount = MilkAmount;

            MilkAmount = 0d;

            OnMilkChanged?.Invoke(MilkAmount);

#if UNITY_EDITOR
            if (_debug)
            {
                Debug.Log(
                    $"[Bucket] Poured: {amount:F2} L | " +
                    $"Remaining: {MilkAmount:F2} / {Capacity:F2} L",
                    this);
            }
#endif

            return amount;
        }
    }
}