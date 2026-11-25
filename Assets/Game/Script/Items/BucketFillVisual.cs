using MilkMaker.Upgrades;
using UnityEngine;

namespace MilkMaker.Items
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Bucket))]
    public sealed class BucketFillVisual : MonoBehaviour
    {
        [Header("Surface")]
        [SerializeField] private Transform _milkSurface;
        [SerializeField] private Transform _emptyPoint;
        [SerializeField] private Transform _fullPoint;

        private Bucket _bucket;
        private UpgradeService _upgrades;

        private void Awake()
        {
            _bucket = GetComponent<Bucket>();
        }

        private void OnEnable()
        {
            _upgrades = _bucket.Upgrades;

            _bucket.OnMilkChanged += OnMilkChanged;

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
            _bucket.OnMilkChanged -= OnMilkChanged;

            if (_upgrades != null)
            {
                _upgrades.OnUpgradePurchased -=
                    OnUpgradePurchased;
            }
        }

        private void OnMilkChanged(double amount)
        {
            Refresh();
        }

        private void OnUpgradePurchased(
            UpgradeProfile profile,
            int level)
        {
            if (profile.Type == UpgradeType.BucketCapacity)
                Refresh();
        }

        private void Refresh()
        {
            if (_milkSurface == null ||
                _emptyPoint == null ||
                _fullPoint == null)
            {
                return;
            }

            _milkSurface.localPosition = Vector3.Lerp(
                _emptyPoint.localPosition,
                _fullPoint.localPosition,
                _bucket.Fill01);

            bool visible = !_bucket.IsEmpty;

            if (_milkSurface.gameObject.activeSelf != visible)
            {
                _milkSurface.gameObject.SetActive(visible);
            }
        }
    }
}