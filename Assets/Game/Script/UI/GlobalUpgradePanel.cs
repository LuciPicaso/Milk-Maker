using MilkMaker.Economy;
using MilkMaker.Modifiers;
using MilkMaker.Upgrades;
using UnityEngine;
using UnityEngine.UIElements;

namespace MilkMaker.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UIDocument))]
    public sealed class GlobalUpgradePanel : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private UpgradeService _upgrades;
        [SerializeField] private IncomeModifiers _modifiers;

        [Header("Stations")]
        [SerializeField] private GlobalUpgradeStation[] _stations;

        [Header("Refresh")]
        [SerializeField, Min(0.05f)]
        private float _refreshInterval = 0.2f;

        private UIDocument _document;
        private VisualElement _root;

        private Label _income;
        private Label _cowCount;

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

            _income = root.Q<Label>("income");
            _cowCount = root.Q<Label>("cow-count");

            _rows = new UpgradeRow[5];

            for (int i = 0; i < _rows.Length; i++)
            {
                VisualElement row =
                    root.Q<VisualElement>($"upgrade-{i}");

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
            if (_upgrades == null ||
                _modifiers == null)
            {
                return;
            }

            SetText(
                _income,
                $"{MilkFormatter.Format(_modifiers.GetPassiveIncome())} L/s");

            int cowCount = GetUnlockedCowCount();

            SetText(
                _cowCount,
                $"COWS UNLOCKED: {cowCount} / {UpgradeService.MaxStalls}");

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
                    cowCount);
            }
        }

        private int GetUnlockedCowCount()
        {
            int count = 0;

            for (int i = 0; i < UpgradeService.MaxStalls; i++)
            {
                if (_upgrades.IsStallUnlocked(i))
                    count++;
            }

            return count;
        }

        private void RefreshRow(
            UpgradeRow row,
            GlobalUpgradeStation station,
            int cowCount)
        {
            if (row == null ||
                station == null ||
                station.Upgrades != _upgrades ||
                station.Profile == null ||
                station.Profile.IsStallUpgrade)
            {
                return;
            }

            UpgradeProfile profile = station.Profile;

            int level = _upgrades.GetLevel(profile);

            SetText(row.Name, profile.DisplayName);

            SetText(
                row.Level,
                profile.Type == UpgradeType.CowUnlock
                    ? $"{cowCount} / {UpgradeService.MaxStalls}"
                    : $"LEVEL {level}");

            if (level >= profile.MaxLevel)
            {
                SetText(row.Price, "MAX");
                SetAvailability(row.Price, true);
                return;
            }

            double price = _upgrades.GetPrice(profile);

            SetText(
                row.Price,
                $"{MilkFormatter.Format(price)} L");

            SetAvailability(
                row.Price,
                _upgrades.CanPurchase(profile));
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