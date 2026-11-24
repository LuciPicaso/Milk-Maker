using MilkMaker.Items;
using MilkMaker.Modifiers;
using MilkMaker.Production;
using MilkMaker.Upgrades;
using UnityEngine;

namespace MilkMaker.Economy
{
    [DisallowMultipleComponent]
    public sealed class PassiveMilkProducer : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private MilkEconomy _economy;
        [SerializeField] private IncomeModifiers _modifiers;
        [SerializeField] private UpgradeService _upgrades;

        [Header("Production")]
        [SerializeField] private BucketSocket _bucketSocket;
        [SerializeField] private MilkConveyor _conveyor;

        [Header("Automation Unlocks")]
        [SerializeField]
        private UpgradeProfile[] _automaticDeliveryUnlocks;

        private double _pendingMilk;

        public double PendingMilk => _pendingMilk;

        public bool IsAutomaticDeliveryUnlocked
        {
            get
            {
                if (_upgrades == null ||
                    _automaticDeliveryUnlocks == null)
                {
                    return false;
                }

                foreach (UpgradeProfile profile
                         in _automaticDeliveryUnlocks)
                {
                    if (profile != null &&
                        _upgrades.GetLevel(profile) > 0)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        private void Update()
        {
            if (_economy == null ||
                !_economy.IsInitialized ||
                _modifiers == null)
            {
                return;
            }

            double produced =
                _modifiers.GetPassiveIncome() *
                Time.deltaTime;

            if (double.IsNaN(produced) ||
                double.IsInfinity(produced) ||
                produced <= 0d)
            {
                return;
            }

            if (IsAutomaticDeliveryUnlocked)
            {
                ProduceAutomatically(produced);
                return;
            }

            ProduceIntoBucket(produced);
        }

        private void ProduceIntoBucket(double amount)
        {
            if (_bucketSocket == null)
                return;

            Bucket bucket = _bucketSocket.OccupiedBucket;

            if (bucket == null || bucket.IsFull)
                return;

            bucket.AddMilk(amount);
        }

        private void ProduceAutomatically(double amount)
        {
            _pendingMilk += amount;

            if (_conveyor == null ||
                _pendingMilk < 1d)
            {
                return;
            }

            if (_conveyor.TrySendAutomatic(_pendingMilk))
                _pendingMilk = 0d;
        }
    }
}