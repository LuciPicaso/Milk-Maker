using MilkMaker.Economy;
using UnityEngine;
using UnityEngine.Events;
using MilkMaker.Upgrades;

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
            _onSlapped.Invoke();

#if UNITY_EDITOR
            if (_debug)
            {
                Debug.Log("[Cow] Slapped", this);
            }
#endif
        }
    }
}