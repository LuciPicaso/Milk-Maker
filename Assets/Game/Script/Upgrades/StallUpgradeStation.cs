using MilkMaker.Farm;
using UnityEngine;

namespace MilkMaker.Upgrades
{
    [DisallowMultipleComponent]
    public sealed class StallUpgradeStation : MonoBehaviour
    {
        [SerializeField] private UpgradeService _upgrades;
        [SerializeField] private CowStall _stall;
        [SerializeField] private UpgradeProfile _profile;
        public UpgradeProfile Profile => _profile;

        public CowStall Stall => _stall;

        public void Buy()
        {
            if (_upgrades == null ||
                _stall == null ||
                !_stall.IsOccupied ||
                _profile == null ||
                !_profile.IsStallUpgrade)
            {
                return;
            }

            _upgrades.TryPurchase(
                _profile,
                _stall.StallId);
        }
    }
}