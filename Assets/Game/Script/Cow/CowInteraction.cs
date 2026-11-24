using MilkMaker.Economy;
using UnityEngine;
using UnityEngine.Events;
using MilkMaker.Upgrades;
using MilkMaker.Modifiers;
using MilkMaker.Items;

namespace MilkMaker.Cow
{
    [DisallowMultipleComponent]
    public sealed class CowInteraction : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private BucketSocket _bucketSocket;

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
            if (_modifiers == null ||
                _bucketSocket == null)
            {
                return;
            }

            Bucket bucket = _bucketSocket.OccupiedBucket;

            if (bucket == null || bucket.IsFull)
                return;

            double amount = _modifiers.GetClickPower();

            double accepted = bucket.AddMilk(amount);

            if (accepted <= 0d)
                return;

            _onMilked.Invoke();

#if UNITY_EDITOR
            if (_debug)
            {
                Debug.Log(
                    $"[Cow] Milked: +{MilkFormatter.Format(accepted)} | " +
                    $"Bucket: {MilkFormatter.Format(bucket.MilkAmount)} / " +
                    $"{MilkFormatter.Format(bucket.Capacity)} L",
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

        public void InitializeAdditionalCow(
            ItemHolder holder,
            IncomeModifiers modifiers,
            ModifierProfile slapProfile,
            UpgradeProfile source,
            int unitIndex)
        {
            _modifiers = modifiers;
            _slapProfile = slapProfile;
            _slapSources = new[] { source };
            _slapUnitIndex = unitIndex;

            if (_bucketSocket != null)
                _bucketSocket.BindHolder(holder);
        }
    }
}