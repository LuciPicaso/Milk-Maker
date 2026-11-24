using MilkMaker.Farm;
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
        [SerializeField] private FarmCowSpawner _farm;
        [SerializeField] private MilkConveyor _conveyor;

        [Header("Automation Unlocks")]
        [SerializeField]
        private UpgradeProfile[] _automaticDeliveryUnlocks;

        public bool IsAutomaticDeliveryUnlocked(int stallId)
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
                    _upgrades.GetLevel(profile, stallId) > 0)
                {
                    return true;
                }
            }

            return false;
        }

        private void Update()
        {
            if (_economy == null ||
                !_economy.IsInitialized ||
                _modifiers == null ||
                _farm == null ||
                _upgrades == null)
            {
                return;
            }

            double deltaTime = Time.deltaTime;

            for (int i = 0; i < UpgradeService.MaxStalls; i++)
            {
                CowStall stall = _farm.GetStall(i);

                if (stall == null ||
                    !stall.IsOccupied ||
                    !_upgrades.IsStallUnlocked(i))
                {
                    continue;
                }

                double produced =
                    _modifiers.GetStallPassiveIncome(i) *
                    deltaTime;

                if (double.IsNaN(produced) ||
                    double.IsInfinity(produced) ||
                    produced <= 0d)
                {
                    continue;
                }

                if (IsAutomaticDeliveryUnlocked(i))
                {
                    if (_conveyor != null)
                        _conveyor.TryEnqueueMilk(produced);
                }
                else
                {
                    ProduceIntoBucket(stall, produced);
                }
            }
        }

        private static void ProduceIntoBucket(
            CowStall stall,
            double amount)
        {
            BucketSocket socket = stall.Cow.BucketSocket;

            if (socket == null)
                return;

            Bucket bucket = socket.OccupiedBucket;

            if (bucket == null || bucket.IsFull)
                return;

            bucket.AddMilk(amount);
        }
    }
}