using System.Collections.Generic;
using MilkMaker.Upgrades;
using UnityEngine;

namespace MilkMaker.Modifiers
{
    [DisallowMultipleComponent]
    public sealed class IncomeModifiers : MonoBehaviour
    {
        [SerializeField] private UpgradeService _upgrades;

        private struct CowBoost
        {
            public UpgradeProfile Source;
            public int UnitIndex;
            public double Multiplier;
            public double ExpireTime;
        }

        private readonly List<CowBoost> _cowBoosts =
            new(16);

        public bool TryActivateCowBoost(
            UpgradeProfile source,
            int unitIndex,
            ModifierProfile profile)
        {
            if (_upgrades == null ||
                source == null ||
                profile == null ||
                !profile.IsValid)
            {
                return false;
            }

            if (_upgrades.GetPassiveSourceIncome(
                    source, unitIndex) <= 0d)
            {
                return false;
            }

            double now = Time.timeAsDouble;

            RemoveExpiredBoosts(now);

            var boost = new CowBoost
            {
                Source = source,
                UnitIndex = unitIndex,
                Multiplier = profile.Multiplier,
                ExpireTime = now + profile.Duration
            };

            for (int i = 0; i < _cowBoosts.Count; i++)
            {
                CowBoost active = _cowBoosts[i];

                if (active.Source != source)
                    continue;

                // Повторный шлепок — обновление таймера.
                if (active.UnitIndex == unitIndex)
                {
                    _cowBoosts[i] = boost;
                    return true;
                }

                // Запрещаем одновременно усиливать
                // весь источник и отдельную его единицу.
                if (active.UnitIndex == 0 || unitIndex == 0)
                    return false;
            }

            _cowBoosts.Add(boost);
            return true;
        }

        public double GetPassiveIncome()
        {
            if (_upgrades == null)
                return 0d;

            RemoveExpiredBoosts(Time.timeAsDouble);

            double income = _upgrades.PassiveIncome;

            for (int i = 0; i < _cowBoosts.Count; i++)
            {
                CowBoost boost = _cowBoosts[i];

                double sourceIncome =
                    _upgrades.GetPassiveSourceIncome(
                        boost.Source,
                        boost.UnitIndex);

                income += sourceIncome *
                          (boost.Multiplier - 1d);
            }

            return income;
        }

        private void RemoveExpiredBoosts(double now)
        {
            for (int i = _cowBoosts.Count - 1; i >= 0; i--)
            {
                if (now >= _cowBoosts[i].ExpireTime)
                    _cowBoosts.RemoveAt(i);
            }
        }
    }
}