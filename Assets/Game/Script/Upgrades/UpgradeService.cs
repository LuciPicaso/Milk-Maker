using System;
using System.Collections.Generic;
using MilkMaker.Economy;
using UnityEngine;

namespace MilkMaker.Upgrades
{
    [DisallowMultipleComponent]
    public sealed class UpgradeService : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private MilkEconomy _economy;

        [Header("Catalog")]
        [SerializeField] private UpgradeProfile[] _profiles;

        private readonly Dictionary<UpgradeProfile, int> _levels =
            new();

        private double _clickBonus;
        private double _passiveBase;
        private bool _isPurchasing;

        public event Action<UpgradeProfile, int> OnUpgradePurchased;

        public double GlobalMultiplier { get; private set; } = 1d;

        public double ClickPower =>
            _economy != null && _economy.IsInitialized
                ? (_economy.Profile.BaseClickPower + _clickBonus)
                  * GlobalMultiplier
                : 0d;

        public double PassiveIncome =>
            _passiveBase * GlobalMultiplier;

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _debug;
#endif

        private void Awake()
        {
            if (_profiles == null)
                return;

            foreach (UpgradeProfile profile in _profiles)
            {
                if (profile == null || _levels.ContainsKey(profile))
                    continue;

                _levels.Add(profile, 0);
            }
        }

        public int GetLevel(UpgradeProfile profile)
        {
            return profile != null &&
                   _levels.TryGetValue(profile, out int level)
                ? level
                : 0;
        }

        public double GetPrice(UpgradeProfile profile)
        {
            if (profile == null ||
                !_levels.TryGetValue(profile, out int level))
            {
                return double.PositiveInfinity;
            }

            return profile.GetPrice(level);
        }

        public bool CanPurchase(UpgradeProfile profile)
        {
            return _economy != null &&
                   _economy.CanAfford(GetPrice(profile));
        }

        // Для UnityEvent и UI Button.
        public void Buy(UpgradeProfile profile)
        {
            TryPurchase(profile);
        }

        public bool TryPurchase(UpgradeProfile profile)
        {
            if (_isPurchasing ||
                !isActiveAndEnabled ||
                _economy == null ||
                profile == null ||
                !_levels.TryGetValue(profile, out int level) ||
                level >= profile.MaxLevel)
            {
                return false;
            }

            double price = profile.GetPrice(level);

            _isPurchasing = true;

            try
            {
                if (!_economy.TrySpend(price))
                    return false;

                int nextLevel = level + 1;

                _levels[profile] = nextLevel;

                RecalculateStats();

                OnUpgradePurchased?.Invoke(profile, nextLevel);

#if UNITY_EDITOR
                if (_debug)
                {
                    Debug.Log(
                        $"[Upgrades] {profile.DisplayName} | " +
                        $"Level: {nextLevel} | " +
                        $"Click: {MilkFormatter.Format(ClickPower)} | " +
                        $"Auto: {MilkFormatter.Format(PassiveIncome)}/s",
                        this);
                }
#endif

                return true;
            }
            finally
            {
                _isPurchasing = false;
            }
        }

        private void RecalculateStats()
        {
            _clickBonus = 0d;
            _passiveBase = 0d;
            GlobalMultiplier = 1d;
            BucketCapacityMultiplier = 1d;

            foreach (var entry in _levels)
            {
                UpgradeProfile profile = entry.Key;
                int level = entry.Value;

                if (level == 0)
                    continue;

                double effect = profile.GetEffect(level);

                switch (profile.Type)
                {
                    case UpgradeType.ClickPower:
                        _clickBonus += effect;
                        break;

                    case UpgradeType.AutoMilker:
                        _passiveBase += effect;
                        break;

                    case UpgradeType.MilkQuality:
                    case UpgradeType.GlobalMultiplier:
                        GlobalMultiplier *= effect;
                        break;

                    case UpgradeType.BucketCapacity:
                        BucketCapacityMultiplier *= effect;
                        break;
                }
            }
        }
        
        public double GetPassiveSourceIncome(
            UpgradeProfile profile,
            int unitIndex = 0)
        {
            if (profile == null ||
                profile.Type != UpgradeType.AutoMilker ||
                unitIndex < 0)
            {
                return 0d;
            }

            int level = GetLevel(profile);

            if (level <= 0 || unitIndex > level)
                return 0d;

            double income =
                profile.GetEffect(level) * GlobalMultiplier;

            return unitIndex == 0
                ? income
                : income / level;
        }

        public double BucketCapacityMultiplier
        {
            get;
            private set;
        } = 1d;
    }
}

