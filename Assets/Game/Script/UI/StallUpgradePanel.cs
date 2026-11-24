using System;
using MilkMaker.Economy;
using MilkMaker.Farm;
using MilkMaker.Modifiers;
using MilkMaker.Upgrades;
using UnityEngine;
using UnityEngine.UIElements;

namespace MilkMaker.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UIDocument))]
    public sealed class StallUpgradePanel : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private CowStall _stall;
        [SerializeField] private UpgradeService _upgrades;
        [SerializeField] private IncomeModifiers _modifiers;

        [Header("Stations")]
        [SerializeField] private StallUpgradeStation[] _stations;

        [Header("Refresh")]
        [SerializeField, Min(0.05f)]
        private float _refreshInterval = 0.2f;

        private UIDocument _document;
        private VisualElement _root;

        private Label _cowName;
        private Label _status;
        private Label _income;
        private Label _slapTimer;

        private sealed class UpgradeRow
        {
            public Label Name;
            public Label Level;
            public Label Price;
        }

        private UpgradeRow[] _rows;
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

            if (_root != root || _rows == null)
                Bind(root);

            Refresh();
        }

        private void Bind(VisualElement root)
        {
            _root = root;

            _cowName = _root.Q<Label>("cow-name");
            _status = _root.Q<Label>("status");
            _income = _root.Q<Label>("income");
            _slapTimer = _root.Q<Label>("slap-timer");

            _rows = new UpgradeRow[3];

            for (int i = 0; i < _rows.Length; i++)
            {
                VisualElement row =
                    _root.Q<VisualElement>($"upgrade-{i}");

                if (row == null)
                    continue;

                _rows[i] = new UpgradeRow
                {
                    Name = row.Q<Label>("upgrade-name"),
                    Level = row.Q<Label>("level"),
                    Price = row.Q<Label>("price")
                };
            }
        }

        private void Refresh()
        {
            if (_stall == null ||
                _upgrades == null ||
                _modifiers == null)
            {
                return;
            }

            int stallId = _stall.StallId;

            if (stallId < 0)
                return;

            bool occupied =
                _stall.IsOccupied &&
                _upgrades.IsStallUnlocked(stallId);

            SetText(
                _cowName,
                $"COW {stallId + 1:00}");

            SetText(
                _status,
                occupied ? "ONLINE" : "LOCKED");

            SetAvailability(_status, occupied);

            double income = occupied
                ? _modifiers.GetStallPassiveIncome(stallId)
                : 0d;

            SetText(
                _income,
                $"{MilkFormatter.Format(income)} L/s");

            double remaining = occupied
                ? _modifiers.GetCowBoostRemaining(stallId)
                : 0d;

            SetText(
                _slapTimer,
                remaining > 0d
                    ? $"SLAP ACTIVE | {Math.Ceiling(remaining)} s"
                    : "SLAP INACTIVE");

            _slapTimer?.EnableInClassList(
                "slap-active",
                remaining > 0d);

            if (_stations == null)
                return;

            int count = Mathf.Min(
                _stations.Length,
                _rows.Length);

            for (int i = 0; i < count; i++)
            {
                RefreshRow(
                    _rows[i],
                    _stations[i],
                    stallId,
                    occupied);
            }
        }

        private void RefreshRow(
            UpgradeRow row,
            StallUpgradeStation station,
            int stallId,
            bool occupied)
        {
            if (row == null ||
                station == null ||
                station.Stall != _stall ||
                station.Profile == null)
            {
                return;
            }

            UpgradeProfile profile = station.Profile;

            int level = _upgrades.GetLevel(
                profile,
                stallId);

            SetText(row.Name, profile.DisplayName);
            SetText(row.Level, $"LEVEL {level}");

            if (!occupied)
            {
                SetText(row.Price, "LOCKED");
                SetAvailability(row.Price, false);
                return;
            }

            if (level >= profile.MaxLevel)
            {
                SetText(row.Price, "MAX");
                SetAvailability(row.Price, true);
                return;
            }

            double price = _upgrades.GetPrice(
                profile,
                stallId);

            SetText(
                row.Price,
                $"{MilkFormatter.Format(price)} L");

            SetAvailability(
                row.Price,
                _upgrades.CanPurchase(profile, stallId));
        }

        private static void SetText(
            Label label,
            string value)
        {
            if (label != null && label.text != value)
                label.text = value;
        }

        private static void SetAvailability(
            Label label,
            bool available)
        {
            if (label == null)
                return;

            label.EnableInClassList(
                "available",
                available);

            label.EnableInClassList(
                "unavailable",
                !available);
        }
    }
}