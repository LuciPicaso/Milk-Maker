using MilkMaker.Economy;
using MilkMaker.Items;
using UnityEngine;
using UnityEngine.Events;

namespace MilkMaker.Production
{
    [DisallowMultipleComponent]
    public sealed class MilkConveyor : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private MilkEconomy _economy;

        [Header("Transport")]
        [SerializeField] private Transform _canVisual;
        [SerializeField] private Transform _startPoint;
        [SerializeField] private Transform _endPoint;

        [SerializeField, Min(0.1f)]
        private float _travelDuration = 2f;

        [SerializeField, Min(0f)]
        private float _automaticDispatchDelay = 0.75f;

        private double _nextAutomaticDispatchTime;
        private double _deliveryRemainder;

        [Header("Presentation")]
        [SerializeField]
        private UnityEvent _onDispatched = new();

        [SerializeField]
        private UnityEvent _onDelivered = new();

        private double _pendingMilk;
        private float _elapsed;

        public bool IsBusy { get; private set; }

        public double PendingMilk => _pendingMilk;

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _debug;
#endif
        
        public bool TrySendAutomatic(double amount)
        {
            if (!isActiveAndEnabled ||
                _economy == null ||
                !_economy.IsInitialized ||
                IsBusy ||
                Time.timeAsDouble < _nextAutomaticDispatchTime ||
                double.IsNaN(amount) ||
                double.IsInfinity(amount) ||
                amount <= 0d)
            {
                return false;
            }

            IsBusy = true;
            _elapsed = 0f;
            _pendingMilk = amount;

            ResetVisual();

            _onDispatched.Invoke();

        #if UNITY_EDITOR
            if (_debug)
            {
                Debug.Log(
                    $"[Conveyor] Auto dispatched: " +
                    $"{MilkFormatter.Format(amount)} L",
                    this);
            }
        #endif

            return true;
        }

        private void Awake()
        {
            if (_economy == null ||
                _canVisual == null ||
                _startPoint == null ||
                _endPoint == null ||
                _travelDuration <= 0f)
            {
                Debug.LogError(
                    "[MilkConveyor] Invalid configuration.",
                    this);

                enabled = false;
                return;
            }

            ResetVisual();
        }

        public bool TrySend(Bucket bucket)
        {
            if (!isActiveAndEnabled ||
                !_economy.IsInitialized ||
                IsBusy ||
                bucket == null ||
                !bucket.IsHeld ||
                bucket.IsEmpty)
            {
                return false;
            }

            IsBusy = true;
            _elapsed = 0f;

            _pendingMilk = bucket.DrainMilk();

            if (_pendingMilk <= 0d)
            {
                IsBusy = false;
                return false;
            }

            ResetVisual();

            _onDispatched.Invoke();

#if UNITY_EDITOR
            if (_debug)
            {
                Debug.Log(
                    $"[Conveyor] Dispatched: " +
                    $"{MilkFormatter.Format(_pendingMilk)} L",
                    this);
            }
#endif

            return true;
        }

        private void Update()
        {
            if (!IsBusy)
                return;

            _elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(
                _elapsed / _travelDuration);

            _canVisual.position = Vector3.Lerp(
                _startPoint.position,
                _endPoint.position,
                progress);

            if (progress >= 1f)
                Deliver();
        }

        private void Deliver()
        {
            double amount = _pendingMilk + _deliveryRemainder;

            bool credited = _economy.AddMilk(amount);

            if (!credited)
            {
                // Сумма слишком мала относительно текущего
                // баланса и не представима в double.
                if (_economy.IsInitialized &&
                    !double.IsNaN(amount) &&
                    !double.IsInfinity(amount) &&
                    amount > 0d &&
                    _economy.Balance + amount == _economy.Balance)
                {
                    _deliveryRemainder = amount;
                }
                else
                {
                    return;
                }
            }
            else
            {
                _deliveryRemainder = 0d;
            }

            _pendingMilk = 0d;
            _elapsed = 0f;
            IsBusy = false;

            _nextAutomaticDispatchTime =
                Time.timeAsDouble + _automaticDispatchDelay;

            ResetVisual();

            _onDelivered.Invoke();

#if UNITY_EDITOR
            if (_debug)
            {
                Debug.Log(
                    $"[Conveyor] Delivered: " +
                    $"{MilkFormatter.Format(amount)} L | " +
                    $"Credited: {credited} | " +
                    $"Balance: " +
                    $"{MilkFormatter.Format(_economy.Balance)} L",
                    this);
            }
#endif
        }

        private void ResetVisual()
        {
            _canVisual.SetPositionAndRotation(
                _startPoint.position,
                _startPoint.rotation);
        }
    }
}