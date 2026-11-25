using MilkMaker.Upgrades;
using UnityEngine;

namespace MilkMaker.Items
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Bucket))]
    public sealed class MilkQualityVisual : MonoBehaviour
    {
        [Header("Quality")]
        [SerializeField] private UpgradeProfile _qualityProfile;

        [Header("Presentation")]
        [SerializeField] private Renderer _milkRenderer;
        [SerializeField] private Material[] _materials;

        private Bucket _bucket;
        private UpgradeService _upgrades;

        private void Awake()
        {
            _bucket = GetComponent<Bucket>();
        }

        private void OnEnable()
        {
            _upgrades = _bucket.Upgrades;

            if (_upgrades != null)
            {
                _upgrades.OnUpgradePurchased +=
                    OnUpgradePurchased;
            }

            Refresh();
        }

        private void Start()
        {
            Refresh();
        }

        private void OnDisable()
        {
            if (_upgrades != null)
            {
                _upgrades.OnUpgradePurchased -=
                    OnUpgradePurchased;
            }
        }

        private void OnUpgradePurchased(
            UpgradeProfile profile,
            int level)
        {
            if (profile == _qualityProfile)
                Refresh();
        }

        private void Refresh()
        {
            if (_milkRenderer == null ||
                _qualityProfile == null ||
                _materials == null ||
                _materials.Length == 0)
            {
                return;
            }

            int level = _upgrades != null
                ? _upgrades.GetLevel(_qualityProfile)
                : 0;

            int index = Mathf.Clamp(
                level,
                0,
                _materials.Length - 1);

            Material material = _materials[index];

            if (material == null)
                return;

            if (_milkRenderer.sharedMaterial != material)
            {
                _milkRenderer.sharedMaterial = material;
            }
        }
    }
}