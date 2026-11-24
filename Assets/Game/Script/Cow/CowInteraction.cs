using MilkMaker.Economy;
using UnityEngine;
using UnityEngine.Events;
using MilkMaker.Upgrades;
using MilkMaker.Modifiers;

namespace MilkMaker.Cow
{
    [DisallowMultipleComponent]
    public sealed class CowInteraction : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private MilkEconomy _economy;
        [SerializeField] private UpgradeService _upgrades;

        [Header("Presentation")]
        [SerializeField]
        private UnityEvent _onMilked = new UnityEvent();

        [SerializeField]
        private UnityEvent _onSlapped = new UnityEvent();

        [Header("Slap Boost")]
        [SerializeField] private IncomeModifiers _modifiers;
        [SerializeField] private ModifierProfile _slapProfile;
        [SerializeField] private UpgradeProfile[] _slapSources;

        [SerializeField, Min(0)]
        private int _slapUnitIndex;

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _debug;
#endif

        public void Milk()
        {
            if (_economy == null ||
                !_economy.IsInitialized)
            {
                return;
            }

            if (_upgrades == null)
                return;

            double amount = _upgrades.ClickPower;

            if (!_economy.AddMilk(amount))
                return;

            _onMilked.Invoke();

#if UNITY_EDITOR
            if (_debug)
            {
                Debug.Log(
                    $"[Cow] Milked: +{amount} | " +
                    $"Balance: {MilkFormatter.Format(_economy.Balance)}",
                    this);
            }
#endif
        }

        public void Slap()
        {
            if (_modifiers != null &&
                _slapProfile != null &&
                _slapSources != null)
            {
                for (int i = 0; i < _slapSources.Length; i++)
                {
                    _modifiers.TryActivateCowBoost(
                        _slapSources[i],
                        _slapUnitIndex,
                        _slapProfile);
                }
            }

            _onSlapped.Invoke();

#if UNITY_EDITOR
            if (_debug)
            {
                Debug.Log(
                    $"[Cow] Slapped | Passive: " +
                    $"{MilkFormatter.Format(_modifiers != null ? _modifiers.GetPassiveIncome():0d)}/s",this);
            }
#endif
        }
    }
}