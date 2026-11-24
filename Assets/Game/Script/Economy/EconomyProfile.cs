using UnityEngine;

namespace MilkMaker.Economy
{
    [CreateAssetMenu(
        fileName = "EconomyProfile",
        menuName = "Milk Maker/Economy/Economy Profile")]
    public sealed class EconomyProfile : ScriptableObject
    {
        [Header("Initial Values")]
        [SerializeField] private double _startingMilk = 0d;
        [SerializeField] private double _baseClickPower = 1d;

        [Header("Goal")]
        [SerializeField] private double _targetMilk = 1e18;

        public double StartingMilk => _startingMilk;
        public double BaseClickPower => _baseClickPower;
        public double TargetMilk => _targetMilk;
    }
}