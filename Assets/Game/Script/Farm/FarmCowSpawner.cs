using MilkMaker.Cow;
using MilkMaker.Items;
using MilkMaker.Modifiers;
using MilkMaker.Upgrades;
using UnityEngine;

namespace MilkMaker.Farm
{
    [DisallowMultipleComponent]
    public sealed class FarmCowSpawner : MonoBehaviour
    {
        private const int MaxCows = 8;

        [Header("Dependencies")]
        [SerializeField] private UpgradeService _upgrades;
        [SerializeField] private IncomeModifiers _modifiers;
        [SerializeField] private ItemHolder _holder;

        [Header("Configuration")]
        [SerializeField] private UpgradeProfile _newCowProfile;
        [SerializeField] private ModifierProfile _slapProfile;

        [Header("Farm")]
        [SerializeField] private CowInteraction _cowPrefab;
        [SerializeField] private CowStall[] _stalls;

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _debug;
#endif

        private void OnEnable()
        {
            if (_upgrades != null)
                _upgrades.OnUpgradePurchased += OnPurchased;

            Refresh();
        }

        private void Start()
        {
            Refresh();
        }

        private void OnDisable()
        {
            if (_upgrades != null)
                _upgrades.OnUpgradePurchased -= OnPurchased;
        }

        private void OnPurchased(
            UpgradeProfile profile,
            int level)
        {
            if (profile == _newCowProfile)
                Refresh();
        }

        public CowStall GetStall(int stallId)
        {
            if (_stalls == null ||
                stallId < 0 ||
                stallId >= _stalls.Length ||
                stallId >= MaxCows)
            {
                return null;
            }

            return _stalls[stallId];
        }

        private void Refresh()
        {
            if (_upgrades == null ||
                _newCowProfile == null ||
                _cowPrefab == null ||
                _modifiers == null ||
                _holder == null ||
                _slapProfile == null ||
                _stalls == null)
            {
                return;
            }

            int count = Mathf.Min(
                _stalls.Length,
                MaxCows);

            for (int i = 0; i < count; i++)
            {
                CowStall stall = _stalls[i];

                if (stall == null)
                    continue;

                stall.BindId(i);

                if (!_upgrades.IsStallUnlocked(i))
                    continue;

                bool wasOccupied = stall.IsOccupied;

                CowInteraction cow = wasOccupied
                    ? stall.Cow
                    : stall.Spawn(_cowPrefab);

                if (cow == null)
                    continue;

                cow.InitializeCow(
                    _holder,
                    _modifiers,
                    _slapProfile,
                    i);

#if UNITY_EDITOR
                if (_debug && !wasOccupied)
                {
                    Debug.Log(
                        $"[FarmCowSpawner] Cow {i + 1} spawned.",
                        this);
                }
#endif
            }
        }
    }
}