using System;
using MilkMaker.Modifiers;
using UnityEngine;

namespace MilkMaker.Pig
{
    [CreateAssetMenu(
        fileName = "PigDealerProfile",
        menuName = "Milk Maker/Pig/Dealer Profile")]
    public sealed class PigDealerProfile : ScriptableObject
    {
        [Header("Appearance")]
        [SerializeField, Min(0.1f)]
        private float _minInterval = 25f;

        [SerializeField, Min(0.1f)]
        private float _maxInterval = 45f;

        [SerializeField, Min(0.1f)]
        private float _offerDuration = 8f;

        [Header("Price")]
        [SerializeField] private double _minPrice = 100d;
        [SerializeField] private double _incomeSeconds = 10d;

        [Header("Product")]
        [SerializeField] private ModifierProfile _pillProfile;

        public float MinInterval => _minInterval;
        public float MaxInterval => _maxInterval;
        public float OfferDuration => _offerDuration;

        public ModifierProfile PillProfile => _pillProfile;

        public bool IsValid =>
            _minInterval > 0f &&
            _maxInterval >= _minInterval &&
            _offerDuration > 0f &&
            !float.IsInfinity(_maxInterval) &&
            !float.IsInfinity(_offerDuration) &&
            _minPrice > 0d &&
            _incomeSeconds >= 0d &&
            !double.IsInfinity(_minPrice) &&
            !double.IsInfinity(_incomeSeconds) &&
            _pillProfile != null &&
            _pillProfile.IsValid;

        public double GetPrice(double passiveIncome)
        {
            return Math.Ceiling(
                Math.Max(
                    _minPrice,
                    passiveIncome * _incomeSeconds));
        }
    }
}