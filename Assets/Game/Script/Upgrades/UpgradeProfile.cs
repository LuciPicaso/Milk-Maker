using System;
using UnityEngine;

namespace MilkMaker.Upgrades
{
    public enum UpgradeType
    {
        ClickPower,
        AutoMilker,
        MilkQuality,
        GlobalMultiplier
    }

    [CreateAssetMenu(
        fileName = "UpgradeProfile",
        menuName = "Milk Maker/Upgrades/Upgrade Profile")]
    public sealed class UpgradeProfile : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _displayName;
        [SerializeField] private UpgradeType _type;

        [Header("Price")]
        [SerializeField] private double _basePrice = 15d;
        [SerializeField] private double _priceGrowth = 1.15d;
        [SerializeField, Min(1)] private int _maxLevel = 100;

        [Header("Effect")]
        [SerializeField] private double _valuePerLevel = 2d;

        [Header("Milestones")]
        [SerializeField, Min(0)] private int _milestoneStep = 10;
        [SerializeField] private double _milestoneMultiplier = 2d;

        [Header("Milk Quality")]
        [SerializeField] private double[] _qualityMultipliers =
        {
            5d, 25d, 100d
        };

        public string DisplayName => _displayName;
        public UpgradeType Type => _type;

        public int MaxLevel =>
            _type == UpgradeType.MilkQuality
                ? Math.Min(_maxLevel, _qualityMultipliers?.Length ?? 0)
                : _maxLevel;

        public double GetPrice(int currentLevel)
        {
            if (currentLevel < 0 || currentLevel >= MaxLevel)
                return double.PositiveInfinity;

            return Math.Ceiling(
                _basePrice * Math.Pow(_priceGrowth, currentLevel));
        }

        public double GetEffect(int level)
        {
            switch (_type)
            {
                case UpgradeType.ClickPower:
                case UpgradeType.AutoMilker:
                {
                    int milestones = _milestoneStep > 0
                        ? level / _milestoneStep
                        : 0;

                    return _valuePerLevel * level *
                           Math.Pow(_milestoneMultiplier, milestones);
                }

                case UpgradeType.MilkQuality:
                    return level > 0
                        ? _qualityMultipliers[level - 1]
                        : 1d;

                case UpgradeType.GlobalMultiplier:
                    return Math.Pow(_valuePerLevel, level);

                default:
                    return 0d;
            }
        }
    }
}