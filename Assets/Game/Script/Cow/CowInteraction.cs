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
        private int _stallId = -1;

        public int StallId => _stallId;

        public BucketSocket BucketSocket => _bucketSocket;

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
                _slapProfile != null)
            {
                _modifiers.TryActivateCowBoost(
                    _stallId,
                    _slapProfile);
            }

            _onSlapped.Invoke();

        #if UNITY_EDITOR
            if (_debug && _modifiers != null)
            {
                Debug.Log(
                    $"[Cow {_stallId + 1}] Slapped | " +
                    $"Income: {MilkFormatter.Format(_modifiers.GetStallPassiveIncome(_stallId))}/s"
                    ,this);
            }
        #endif
        }

        public void InitializeCow(
            ItemHolder holder,
            IncomeModifiers modifiers,
            ModifierProfile slapProfile,
            int stallId)
        {
            _modifiers = modifiers;
            _slapProfile = slapProfile;
            _stallId = stallId;

            if (_bucketSocket != null)
                _bucketSocket.BindHolder(holder);
        }
    }
}