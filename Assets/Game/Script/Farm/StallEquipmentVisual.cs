using System;
using MilkMaker.Upgrades;
using UnityEngine;

namespace MilkMaker.Farm
{
    [DisallowMultipleComponent]
    public sealed class StallEquipmentVisual : MonoBehaviour
    {
        [Serializable]
        private sealed class EquipmentEntry
        {
            public UpgradeProfile Profile;

            [Min(1)]
            public int RequiredLevel = 1;

            public GameObject Target;
        }

        [Header("Dependencies")]
        [SerializeField] private UpgradeService _upgrades;
        [SerializeField] private CowStall _stall;

        [Header("Equipment")]
        [SerializeField] private EquipmentEntry[] _entries;

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _debug;
#endif

        private void OnEnable()
        {
            if (_upgrades != null)
            {
                _upgrades.OnStallUpgradePurchased +=
                    OnStallUpgradePurchased;
            }

            RefreshAll();
        }

        private void Start()
        {
            RefreshAll();
        }

        private void OnDisable()
        {
            if (_upgrades != null)
            {
                _upgrades.OnStallUpgradePurchased -=
                    OnStallUpgradePurchased;
            }
        }

        private void OnStallUpgradePurchased(
            UpgradeProfile profile,
            int stallId,
            int level)
        {
            if (_stall == null ||
                _stall.StallId != stallId ||
                _entries == null)
            {
                return;
            }

            foreach (EquipmentEntry entry in _entries)
            {
                if (entry == null ||
                    entry.Profile != profile)
                {
                    continue;
                }

                Apply(entry, level);
            }
        }

        private void RefreshAll()
        {
            if (_upgrades == null ||
                _stall == null ||
                _entries == null)
            {
                return;
            }

            foreach (EquipmentEntry entry in _entries)
            {
                if (entry == null ||
                    entry.Profile == null ||
                    !entry.Profile.IsStallUpgrade)
                {
                    continue;
                }

                int level = _upgrades.GetLevel(
                    entry.Profile,
                    _stall.StallId);

                Apply(entry, level);
            }
        }

        private void Apply(
            EquipmentEntry entry,
            int level)
        {
            if (entry.Target == null)
                return;

            bool unlocked =
                level >= entry.RequiredLevel;

            if (entry.Target.activeSelf == unlocked)
                return;

            entry.Target.SetActive(unlocked);

#if UNITY_EDITOR
            if (_debug && unlocked)
            {
                Debug.Log(
                    $"[StallEquipment] " +
                    $"Stall: {_stall.StallId + 1} | " +
                    $"Unlocked: {entry.Target.name} | " +
                    $"Level: {level}",
                    this);
            }
#endif
        }
    }
}