using MilkMaker.Upgrades;
using UnityEngine;

namespace MilkMaker.Economy
{
    [DisallowMultipleComponent]
    public sealed class PassiveMilkProducer : MonoBehaviour
    {
        [SerializeField] private MilkEconomy _economy;
        [SerializeField] private UpgradeService _upgrades;

        [SerializeField, Min(0.01f)]
        private float _tickInterval = 0.25f;

        private double _pendingMilk;
        private double _elapsed;

        private void Update()
        {
            if (_economy == null ||
                !_economy.IsInitialized ||
                _upgrades == null)
            {
                return;
            }

            double deltaTime = Time.deltaTime;

            _pendingMilk += _upgrades.PassiveIncome * deltaTime;
            _elapsed += deltaTime;

            if (_elapsed < _tickInterval)
                return;

            _elapsed %= _tickInterval;

            if (_pendingMilk <= 0d)
                return;

            if (_economy.AddMilk(_pendingMilk))
                _pendingMilk = 0d;
        }
    }
}