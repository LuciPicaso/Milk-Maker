using System;
using MilkMaker.Economy;
using MilkMaker.Pig;
using UnityEngine;
using UnityEngine.UIElements;

namespace MilkMaker.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UIDocument))]
    public sealed class PigDealerPanel : MonoBehaviour
    {
        [SerializeField] private PigDealer _dealer;

        [SerializeField, Min(0.05f)]
        private float _refreshInterval = 0.1f;

        private UIDocument _document;
        private VisualElement _root;

        private Label _multiplier;
        private Label _duration;
        private Label _price;
        private Label _offerTimer;
        private Label _purchaseStatus;

        private VisualElement _offerFill;

        private float _elapsed;

        private void Awake()
        {
            _document = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            _root = null;
            _elapsed = _refreshInterval;
        }

        private void Update()
        {
            _elapsed += Time.unscaledDeltaTime;

            if (_elapsed < _refreshInterval)
                return;

            _elapsed = 0f;

            VisualElement root = _document.rootVisualElement;

            if (root == null)
                return;

            if (_root != root)
                Bind(root);

            Refresh();
        }

        private void Bind(VisualElement root)
        {
            _root = root;

            _multiplier = root.Q<Label>("multiplier");
            _duration = root.Q<Label>("duration");
            _price = root.Q<Label>("price");
            _offerTimer = root.Q<Label>("offer-timer");

            _purchaseStatus =
                root.Q<Label>("purchase-status");

            _offerFill =
                root.Q<VisualElement>("offer-fill");
        }

        private void Refresh()
        {
            if (_dealer == null ||
                !_dealer.IsOfferOpen ||
                _dealer.Profile == null ||
                _dealer.Profile.PillProfile == null)
            {
                return;
            }

            var profile = _dealer.Profile;
            var pill = profile.PillProfile;

            SetText(
                _multiplier,
                $"x{pill.Multiplier:0.##} INCOME");

            SetText(
                _duration,
                $"Duration: {pill.Duration:0.##} s");

            SetText(
                _price,
                $"{MilkFormatter.Format(_dealer.OfferPrice)} L");

            double remaining = _dealer.OfferRemaining;

            SetText(
                _offerTimer,
                $"OFFER EXPIRES: {Math.Ceiling(remaining)} s");

            if (_offerFill != null)
            {
                float progress = Mathf.Clamp01(
                    (float)(remaining / profile.OfferDuration));

                _offerFill.style.width =
                    Length.Percent(progress * 100f);
            }

            bool canBuy = _dealer.CanBuyPill;

            SetText(
                _purchaseStatus,
                canBuy ? "BUY PILLS" : "NOT AVAILABLE");

            _purchaseStatus?.EnableInClassList(
                "purchase-unavailable",
                !canBuy);
        }

        private static void SetText(
            Label label,
            string value)
        {
            if (label != null && label.text != value)
                label.text = value;
        }
    }
}