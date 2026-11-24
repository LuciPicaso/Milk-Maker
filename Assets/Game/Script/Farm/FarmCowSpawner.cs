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

            int purchased =
                _upgrades.GetLevel(_newCowProfile);

            int count = Mathf.Min(
                purchased + 1,
                MaxCows,
                _stalls.Length);

            // Стойло 0 уже содержит стартовую корову.
            for (int i = 1; i < count; i++)
            {
                CowStall stall = _stalls[i];

                if (stall == null || stall.IsOccupied)
                    continue;

                CowInteraction cow =
                    stall.Spawn(_cowPrefab);

                if (cow == null)
                    continue;

                cow.InitializeAdditionalCow(
                    _holder,
                    _modifiers,
                    _slapProfile,
                    _newCowProfile,
                    i);

#if UNITY_EDITOR
                if (_debug)
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