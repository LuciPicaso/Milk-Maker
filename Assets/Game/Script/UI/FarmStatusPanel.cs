using System;
using MilkMaker.Economy;
using MilkMaker.Items;
using UnityEngine;
using UnityEngine.UIElements;
using MilkMaker.Modifiers;

namespace MilkMaker.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UIDocument))]
    public sealed class FarmStatusPanel : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private MilkEconomy _economy;
        [SerializeField] private Bucket _bucket;
        [SerializeField] private IncomeModifiers _modifiers;

        [Header("Refresh")]
        [SerializeField, Min(0.05f)]
        private float _refreshInterval = 0.2f;

        private VisualElement _boostBanner;
        private VisualElement _boostFill;

        private Label _boostMultiplier;
        private Label _boostTimer;

        private UIDocument _document;
        private VisualElement _root;

        private Label _balance;
        private Label _target;
        private Label _remainingDebt;
        private Label _debtPercent;

        private Label _bucketAmount;

        private VisualElement _bucketFill;
        private VisualElement _debtFill;

        private double _lastBalance;
        private double _lastTarget;

        private double _lastBucketAmount;
        private double _lastBucketCapacity;

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

            RefreshEconomy();
            RefreshBucket();
            RefreshBoost();
        }

        private void Bind(VisualElement root)
        {
            _root = root;

            _balance = root.Q<Label>("balance");
            _target = root.Q<Label>("target");

            _remainingDebt =
                root.Q<Label>("remaining-debt");

            _debtPercent =
                root.Q<Label>("debt-percent");

            _debtFill =
                root.Q<VisualElement>("debt-fill");

            _bucketAmount =
                root.Q<Label>("bucket-amount");

            _bucketFill =
                root.Q<VisualElement>("bucket-fill");

            _boostBanner =
                root.Q<VisualElement>("boost-banner");

            _boostFill =
                root.Q<VisualElement>("boost-fill");

            _boostMultiplier =
                root.Q<Label>("boost-multiplier");

            _boostTimer =
                root.Q<Label>("boost-timer");

            _lastBalance = double.NaN;
            _lastTarget = double.NaN;

            _lastBucketAmount = double.NaN;
            _lastBucketCapacity = double.NaN;
        }

        private void RefreshEconomy()
        {
            if (_economy == null ||
                !_economy.IsInitialized)
            {
                return;
            }

            double balance = _economy.Balance;
            double target = _economy.Profile.TargetMilk;

            if (balance == _lastBalance &&
                target == _lastTarget)
            {
                return;
            }

            _lastBalance = balance;
            _lastTarget = target;

            if (_balance != null)
            {
                _balance.text =
                    $"{MilkFormatter.Format(balance)} L";
            }

            if (_target != null)
            {
                _target.text =
                    $"{MilkFormatter.Format(target)} L";
            }

            double remaining = Math.Max(
                0d,
                target - balance);

            if (_remainingDebt != null)
            {
                _remainingDebt.text =
                    $"{MilkFormatter.Format(remaining)} L";
            }

            double progress = Math.Min(
                1d,
                balance / target);

            if (_debtFill != null)
            {
                _debtFill.style.width =
                    Length.Percent((float)(progress * 100d));
            }

            if (_debtPercent != null)
            {
                double percent = _economy.IsGoalReached
                    ? 100d
                    : Math.Min(99.99d, progress * 100d);

                _debtPercent.text = $"{percent:F2}%";
            }
        }

        private void RefreshBucket()
        {
            if (_bucket == null)
                return;

            double amount = _bucket.MilkAmount;
            double capacity = _bucket.Capacity;

            if (amount == _lastBucketAmount &&
                capacity == _lastBucketCapacity)
            {
                return;
            }

            _lastBucketAmount = amount;
            _lastBucketCapacity = capacity;

            if (_bucketAmount != null)
            {
                _bucketAmount.text =
                    $"{MilkFormatter.Format(amount)} / " +
                    $"{MilkFormatter.Format(capacity)} L";
            }

            if (_bucketFill != null)
            {
                _bucketFill.style.width =
                    Length.Percent(_bucket.Fill01 * 100f);
            }
        }

        private void RefreshBoost()
        {
            if (_boostBanner == null)
                return;

            bool active =
                _modifiers != null &&
                _modifiers.IsGlobalBoostActive;

            _boostBanner.EnableInClassList(
                "boost-active",
                active);

            if (!active)
                return;

            double remaining =
                _modifiers.GlobalBoostRemaining;

            if (_boostMultiplier != null)
            {
                _boostMultiplier.text =
                    $"BOOST x{_modifiers.GlobalBoostMultiplier:0.##}";
            }

            if (_boostTimer != null)
            {
                _boostTimer.text =
                    $"{Math.Ceiling(remaining)} s";
            }

            float duration =
                _modifiers.GlobalBoostDuration;

            if (_boostFill != null && duration > 0f)
            {
                float progress = Mathf.Clamp01(
                    (float)(remaining / duration));

                _boostFill.style.width =
                    Length.Percent(progress * 100f);
            }
        }
    }
}