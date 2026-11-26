using System;
using System.Collections.Generic;
using MilkMaker.Economy;
using UnityEngine;
using UnityEngine.Events;

namespace MilkMaker.Upgrades
{
    [DisallowMultipleComponent]
    public sealed class UpgradeService : MonoBehaviour
    {
        public const int MaxStalls = 8;
        public const int GlobalId = -1;

        [Header("Automation Unlocks")]
        [SerializeField]
        private UpgradeProfile[] _automaticDeliveryUnlocks;

        [Header("Presentation")]
        [SerializeField] private UnityEvent _onPurchased = new();

        [Header("Dependencies")]
        [SerializeField] private MilkEconomy _economy;

        [Header("Catalog")]
        [SerializeField] private UpgradeProfile[] _profiles;
        [SerializeField] private UpgradeProfile _newCowProfile;

        private readonly Dictionary<
            (UpgradeProfile Profile, int StallId), int> _levels =
            new();

        private readonly double[] _stallPassiveBase =
            new double[MaxStalls];

        private double _clickBonus;
        private double _passiveBase;
        private bool _isPurchasing;

        public event Action<UpgradeProfile, int> OnUpgradePurchased;

        public event Action<UpgradeProfile, int, int>
            OnStallUpgradePurchased;

        public double GlobalMultiplier { get; private set; } = 1d;

        public double BucketCapacityMultiplier
        {
            get;
            private set;
        } = 1d;

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
                if (profile == null)
                    continue;

                if (profile.IsStallUpgrade)
                {
                    for (int i = 0; i < MaxStalls; i++)
                        _levels.TryAdd((profile, i), 0);
                }
                else
                {
                    _levels.TryAdd((profile, GlobalId), 0);
                }
            }
        }

        public bool IsStallUnlocked(int stallId)
        {
            if (stallId < 0 || stallId >= MaxStalls)
                return false;

            return stallId <= GetLevel(_newCowProfile);
        }

        public int GetLevel(UpgradeProfile profile)
        {
            return GetLevel(profile, GlobalId);
        }

        public int GetLevel(
            UpgradeProfile profile,
            int stallId)
        {
            return profile != null &&
                   _levels.TryGetValue(
                       (profile, stallId), out int level)
                ? level
                : 0;
        }

        public double GetPrice(UpgradeProfile profile)
        {
            return GetPrice(profile, GlobalId);
        }

        public double GetPrice(
            UpgradeProfile profile,
            int stallId)
        {
            if (profile == null ||
                !_levels.TryGetValue(
                    (profile, stallId), out int level))
            {
                return double.PositiveInfinity;
            }

            if (profile.IsStallUpgrade &&
                !IsStallUnlocked(stallId))
            {
                return double.PositiveInfinity;
            }

            return profile.GetPrice(level);
        }

        public bool CanPurchase(UpgradeProfile profile)
        {
            return CanPurchase(profile, GlobalId);
        }

        public bool CanPurchase(
            UpgradeProfile profile,
            int stallId)
        {
            return isActiveAndEnabled &&
                   _economy != null &&
                   _economy.CanAfford(
                       GetPrice(profile, stallId));
        }

        public void Buy(UpgradeProfile profile)
        {
            TryPurchase(profile);
        }

        public bool TryPurchase(UpgradeProfile profile)
        {
            return TryPurchase(profile, GlobalId);
        }

        public bool TryPurchase(
            UpgradeProfile profile,
            int stallId)
        {
            if (_isPurchasing ||
                !CanPurchase(profile, stallId))
            {
                return false;
            }

            var key = (profile, stallId);

            int level = _levels[key];

            if (level >= profile.MaxLevel)
                return false;

            double price = GetPrice(profile, stallId);

            _isPurchasing = true;

            try
            {
                if (!_economy.TrySpend(price))
                    return false;

                int nextLevel = level + 1;

                _levels[key] = nextLevel;

                RecalculateStats();

                if (profile.IsStallUpgrade)
                {
                    OnStallUpgradePurchased?.Invoke(
                        profile, stallId, nextLevel);
                }
                else
                {
                    OnUpgradePurchased?.Invoke(
                        profile, nextLevel);
                }

                _onPurchased.Invoke();

#if UNITY_EDITOR
                if (_debug)
                {
                    Debug.Log(
                        $"[Upgrades] {profile.DisplayName} | " +
                        $"Stall: {stallId} | " +
                        $"Level: {nextLevel}",
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

        public double GetStallPassiveIncome(int stallId)
        {
            if (!IsStallUnlocked(stallId))
                return 0d;

            return _stallPassiveBase[stallId] *
                   GlobalMultiplier;
        }

        private void RecalculateStats()
        {
            _clickBonus = 0d;
            _passiveBase = 0d;
            GlobalMultiplier = 1d;
            BucketCapacityMultiplier = 1d;

            Array.Clear(
                _stallPassiveBase,
                0,
                _stallPassiveBase.Length);

            foreach (var entry in _levels)
            {
                UpgradeProfile profile = entry.Key.Profile;
                int stallId = entry.Key.StallId;
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
                        _stallPassiveBase[stallId] += effect;
                        _passiveBase += effect;
                        break;

                    case UpgradeType.MilkQuality:
                    case UpgradeType.GlobalMultiplier:
                        GlobalMultiplier *= effect;
                        break;

                    case UpgradeType.BucketCapacity:
                        BucketCapacityMultiplier *= effect;
                        break;

                    case UpgradeType.CowUnlock:
                        break;
                }
            }
        }

        public bool IsAutomaticDeliveryUnlocked(int stallId)
        {
            if (!IsStallUnlocked(stallId) ||
                _automaticDeliveryUnlocks == null)
            {
                return false;
            }

            foreach (UpgradeProfile profile in _automaticDeliveryUnlocks)
            {
                if (profile != null &&
                    GetLevel(profile, stallId) > 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}