using System;
using MilkMaker.Upgrades;
using UnityEngine;

namespace MilkMaker.Farm
{
    [DisallowMultipleComponent]
    public sealed class FarmExpander : MonoBehaviour
    {
        [Serializable]
        private sealed class ExpansionEntry
        {
            public UpgradeProfile Profile;

            [Min(1)]
            public int RequiredLevel = 1;

            public GameObject Target;
        }

        [SerializeField]
        private UpgradeService _upgrades;

        [SerializeField]
        private ExpansionEntry[] _entries;

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _debug;
#endif

        private void OnEnable()
        {
            if (_upgrades != null)
                _upgrades.OnUpgradePurchased += OnUpgradePurchased;

            RefreshAll();
        }

        private void Start()
        {
            RefreshAll();
        }

        private void OnDisable()
        {
            if (_upgrades != null)
                _upgrades.OnUpgradePurchased -= OnUpgradePurchased;
        }

        private void OnUpgradePurchased(
            UpgradeProfile profile,
            int level)
        {
            if (_entries == null)
                return;

            foreach (ExpansionEntry entry in _entries)
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
            if (_upgrades == null || _entries == null)
                return;

            foreach (ExpansionEntry entry in _entries)
            {
                if (entry == null || entry.Profile == null)
                    continue;

                Apply(
                    entry,
                    _upgrades.GetLevel(entry.Profile));
            }
        }

        private void Apply(
            ExpansionEntry entry,
            int level)
        {
            if (entry.Target == null)
                return;

            bool unlocked = level >= entry.RequiredLevel;

            if (entry.Target.activeSelf == unlocked)
                return;

            entry.Target.SetActive(unlocked);

#if UNITY_EDITOR
            if (_debug && unlocked)
            {
                Debug.Log(
                    $"[FarmExpander] Unlocked: " +
                    $"{entry.Target.name} | Level: {level}",
                    this);
            }
#endif
        }
    }
}