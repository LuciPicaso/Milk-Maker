using UnityEngine;

namespace MilkMaker.Modifiers
{
    [CreateAssetMenu(
        fileName = "ModifierProfile",
        menuName = "Milk Maker/Modifiers/Modifier Profile")]
    public sealed class ModifierProfile : ScriptableObject
    {
        [SerializeField] private double _multiplier = 2d;

        [SerializeField, Min(0.01f)]
        private float _duration = 8f;

        public double Multiplier => _multiplier;
        public float Duration => _duration;

        public bool IsValid =>
            !double.IsNaN(_multiplier) &&
            !double.IsInfinity(_multiplier) &&
            _multiplier >= 1d &&
            !float.IsNaN(_duration) &&
            !float.IsInfinity(_duration) &&
            _duration > 0f;
    }
}