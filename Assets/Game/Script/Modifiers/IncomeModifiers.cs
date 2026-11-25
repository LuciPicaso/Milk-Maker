using MilkMaker.Upgrades;
using UnityEngine;
using MilkMaker.Economy;

namespace MilkMaker.Modifiers
{
    [DisallowMultipleComponent]
    public sealed class IncomeModifiers : MonoBehaviour
    {
        [SerializeField] private UpgradeService _upgrades;

        public float GlobalBoostDuration { get; private set; }

        private double _globalBoostMultiplier = 1d;
        private double _globalBoostExpireTime;
        private bool _isPurchasingGlobalBoost;

        public bool IsGlobalBoostActive =>
            Time.timeAsDouble < _globalBoostExpireTime;

        public double GlobalBoostMultiplier =>
            IsGlobalBoostActive ? _globalBoostMultiplier : 1d;

        public double GlobalBoostRemaining =>
            System.Math.Max(
                0d,
                _globalBoostExpireTime - Time.timeAsDouble);
        
        private readonly double[] _slapExpireTimes =
            new double[UpgradeService.MaxStalls];

        private readonly double[] _slapMultipliers =
            new double[UpgradeService.MaxStalls];

        public bool IsCowBoostActive(int stallId)
        {
            return stallId >= 0 &&
                stallId < UpgradeService.MaxStalls &&
                Time.timeAsDouble < _slapExpireTimes[stallId];
        }

        public double GetCowBoostRemaining(int stallId)
        {
            if (!IsCowBoostActive(stallId))
                return 0d;

            return System.Math.Max(
                0d,
                _slapExpireTimes[stallId] - Time.timeAsDouble);
        }

        public bool TryActivateCowBoost(
            int stallId,
            ModifierProfile profile)
        {
            if (!isActiveAndEnabled ||
                _upgrades == null ||
                profile == null ||
                !profile.IsValid ||
                !_upgrades.IsStallUnlocked(stallId))
            {
                return false;
            }

            if (_upgrades.GetStallPassiveIncome(stallId) <= 0d)
                return false;

            _slapMultipliers[stallId] = profile.Multiplier;

            _slapExpireTimes[stallId] =
                Time.timeAsDouble + profile.Duration;

            return true;
        }

        public double GetStallPassiveIncome(int stallId)
        {
            if (_upgrades == null ||
                !_upgrades.IsStallUnlocked(stallId))
            {
                return 0d;
            }

            double slapMultiplier = IsCowBoostActive(stallId)
                ? _slapMultipliers[stallId]
                : 1d;

            return _upgrades.GetStallPassiveIncome(stallId) *
                slapMultiplier *
                GlobalBoostMultiplier;
        }

        public double GetPassiveIncome()
        {
            double income = 0d;

            for (int i = 0; i < UpgradeService.MaxStalls; i++)
                income += GetStallPassiveIncome(i);

            return income;
        }

        public bool TryPurchaseGlobalBoost(
            MilkEconomy economy,
            ModifierProfile profile,
            double price)
        {
            if (!isActiveAndEnabled ||
                _isPurchasingGlobalBoost ||
                IsGlobalBoostActive ||
                economy == null ||
                profile == null ||
                !profile.IsValid)
            {
                return false;
            }

            double multiplier = profile.Multiplier;
            float duration = profile.Duration;

            _isPurchasingGlobalBoost = true;

            try
            {
                if (!economy.TrySpend(price))
                    return false;

                _globalBoostMultiplier = multiplier;
                
                GlobalBoostDuration = duration;

                _globalBoostExpireTime =
                    Time.timeAsDouble + duration;

                return true;
            }
            finally
            {
                _isPurchasingGlobalBoost = false;
            }
            
        }

        public double GetClickPower()
        {
            if (_upgrades == null)
                return 0d;

            return _upgrades.ClickPower * GlobalBoostMultiplier;
        }
    }
}