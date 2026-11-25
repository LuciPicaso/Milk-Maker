using System;
using UnityEngine;

namespace MilkMaker.Economy
{
    [DisallowMultipleComponent]
    public sealed class MilkEconomy : MonoBehaviour
    {
        [SerializeField] private EconomyProfile _profile;

        private double _balance;

        public event Action<double> OnBalanceChanged;

        public double Balance => _balance;
        public bool IsInitialized { get; private set; }

        public EconomyProfile Profile => _profile;

        public bool IsGoalReached =>
            IsInitialized && _balance >= _profile.TargetMilk;

        private void Awake()
        {
            if (_profile == null)
            {
                Debug.LogError(
                    "[MilkEconomy] EconomyProfile is missing.",
                    this);

                enabled = false;
                return;
            }

            if (!IsValidAmount(_profile.StartingMilk) ||
                !IsValidAmount(_profile.BaseClickPower) ||
                !IsValidAmount(_profile.TargetMilk) ||
                _profile.BaseClickPower <= 0d ||
                _profile.TargetMilk <= 0d)
            {
                Debug.LogError(
                    "[MilkEconomy] Invalid profile values.",
                    this);

                enabled = false;
                return;
            }

            _balance = _profile.StartingMilk;
            IsInitialized = true;
        }

        public bool AddMilk(double amount)
        {
            if (!IsInitialized ||
                !IsValidAmount(amount) ||
                amount <= 0d)
            {
                return false;
            }

            double nextBalance = _balance + amount;

            if (!IsValidAmount(nextBalance) ||
                nextBalance == _balance)
            {
                return false;
            }

            SetBalance(nextBalance);
            return true;
        }

        public bool CanAfford(double amount)
        {
            return IsInitialized &&
                   IsValidAmount(amount) &&
                   amount > 0d &&
                   _balance >= amount;
        }

        public bool TrySpend(double amount)
        {
            if (!CanAfford(amount))
                return false;

            double nextBalance = _balance - amount;

            if (nextBalance == _balance)
                return false;

            SetBalance(nextBalance);
            return true;
        }

        private void SetBalance(double value)
        {
            _balance = value;
            OnBalanceChanged?.Invoke(_balance);
        }

        private static bool IsValidAmount(double value)
        {
            return !double.IsNaN(value) &&
                   !double.IsInfinity(value) &&
                   value >= 0d;
        }

#if UNITY_EDITOR

        [ContextMenu("Debug/Add 10000000000 Milk")]
        private void DebugAddMilk()
        {
            if (!Application.isPlaying)
                return;

            AddMilk(100000000000000d);

            Debug.Log(
                $"[MilkEconomy] Balance: " +
                $"{MilkFormatter.Format(_balance)}",
                this);
        }

        [ContextMenu("Debug/Spend 15 Milk")]
        private void DebugSpendMilk()
        {
            if (!Application.isPlaying)
                return;

            bool success = TrySpend(15d);

            Debug.Log(
                $"[MilkEconomy] Purchase: {success} | " +
                $"Balance: {MilkFormatter.Format(_balance)}",
                this);
        }

        [ContextMenu("Debug/Add Goal Amount")]
        private void DebugAddGoalAmount()
        {
            if (!Application.isPlaying || !IsInitialized)
                return;

            AddMilk(_profile.TargetMilk);

            Debug.Log(
                $"[MilkEconomy] Goal reached: {IsGoalReached}",
                this);
        }

#endif
    }
}