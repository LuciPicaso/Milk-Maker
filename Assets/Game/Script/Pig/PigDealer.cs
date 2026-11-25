using MilkMaker.Economy;
using MilkMaker.Modifiers;
using MilkMaker.Upgrades;
using UnityEngine;

namespace MilkMaker.Pig
{
    [DisallowMultipleComponent]
    public sealed class PigDealer : MonoBehaviour
    {
        private enum DealerState
        {
            Waiting,
            Trading
        }

        [Header("Dependencies")]
        [SerializeField] private MilkEconomy _economy;
        [SerializeField] private UpgradeService _upgrades;
        [SerializeField] private IncomeModifiers _modifiers;

        [Header("Configuration")]
        [SerializeField] private PigDealerProfile _profile;

        [Header("Presentation")]
        [SerializeField] private GameObject _offerVisual;

        private DealerState _state;
        private double _nextStateTime;

        public double OfferPrice { get; private set; }

        public bool IsOfferOpen =>
            _state == DealerState.Trading &&
            Time.timeAsDouble < _nextStateTime;

        public PigDealerProfile Profile => _profile;

        public double OfferRemaining =>
            IsOfferOpen
                ? System.Math.Max(
                    0d,
                    _nextStateTime - Time.timeAsDouble)
                : 0d;

        public bool CanBuyPill =>
            isActiveAndEnabled &&
            IsOfferOpen &&
            _economy != null &&
            _modifiers != null &&
            _modifiers.isActiveAndEnabled &&
            !_modifiers.IsGlobalBoostActive &&
            _economy.CanAfford(OfferPrice);

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _debug;
#endif

        private void OnEnable()
        {
            if (_economy == null ||
                _upgrades == null ||
                _modifiers == null ||
                _profile == null ||
                !_profile.IsValid ||
                _offerVisual == null ||
                _offerVisual == gameObject ||
                !_offerVisual.transform.IsChildOf(transform))
            {
                Debug.LogError(
                    "[PigDealer] Invalid configuration.",
                    this);

                enabled = false;
                return;
            }

            ScheduleNextOffer();
        }

        private void OnDisable()
        {
            _state = DealerState.Waiting;

            if (_offerVisual != null &&
                _offerVisual != gameObject &&
                _offerVisual.transform.IsChildOf(transform))
            {
                _offerVisual.SetActive(false);
            }
        }

        private void Update()
        {
            if (Time.timeAsDouble < _nextStateTime)
                return;

            switch (_state)
            {
                case DealerState.Waiting:
                    OpenOffer();
                    break;

                case DealerState.Trading:
                    ScheduleNextOffer();
                    break;
            }
        }

        private void OpenOffer()
        {
            if (!_economy.IsInitialized ||
                _modifiers.IsGlobalBoostActive)
            {
                ScheduleNextOffer();
                return;
            }

            OfferPrice =
                _profile.GetPrice(_upgrades.PassiveIncome);

            _state = DealerState.Trading;

            _nextStateTime =
                Time.timeAsDouble + _profile.OfferDuration;

            _offerVisual.SetActive(true);

#if UNITY_EDITOR
            if (_debug)
            {
                Debug.Log(
                    $"[PigDealer] Offer: " +
                    $"{MilkFormatter.Format(OfferPrice)} L",
                    this);
            }
#endif
        }

        public void BuyPill()
        {
            if (!isActiveAndEnabled || !IsOfferOpen)
                return;

            bool purchased =
                _modifiers.TryPurchaseGlobalBoost(
                    _economy,
                    _profile.PillProfile,
                    OfferPrice);

            if (!purchased)
            {
#if UNITY_EDITOR
                if (_debug)
                    Debug.Log("[PigDealer] Purchase rejected.", this);
#endif
                return;
            }

#if UNITY_EDITOR
            if (_debug)
            {
                Debug.Log(
                    $"[PigDealer] Purchased | " +
                    $"Boost: x{_modifiers.GlobalBoostMultiplier}",
                    this);
            }
#endif

            ScheduleNextOffer();
        }

        private void ScheduleNextOffer()
        {
            _state = DealerState.Waiting;
            OfferPrice = 0d;

            _nextStateTime =
                Time.timeAsDouble +
                Random.Range(
                    _profile.MinInterval,
                    _profile.MaxInterval);

            _offerVisual.SetActive(false);
        }
    }
}