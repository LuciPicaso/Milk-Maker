using UnityEngine;

namespace MilkMaker.Upgrades
{
    [DisallowMultipleComponent]
    public sealed class GlobalUpgradeStation : MonoBehaviour
    {
        [SerializeField] private UpgradeService _upgrades;
        [SerializeField] private UpgradeProfile _profile;

        public UpgradeProfile Profile => _profile;
        public UpgradeService Upgrades => _upgrades;

        public void Buy()
        {
            if (_upgrades == null ||
                _profile == null ||
                _profile.IsStallUpgrade)
            {
                return;
            }

            _upgrades.TryPurchase(_profile);
        }
    }
}