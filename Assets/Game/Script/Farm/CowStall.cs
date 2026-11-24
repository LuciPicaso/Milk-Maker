using MilkMaker.Cow;
using UnityEngine;

namespace MilkMaker.Farm
{
    [DisallowMultipleComponent]
    public sealed class CowStall : MonoBehaviour
    {
        [SerializeField] private Transform _spawnPoint;

        [SerializeField] private CowInteraction _cow;

        public CowInteraction Cow => _cow;

        public bool IsOccupied => _cow != null;

        public CowInteraction Spawn(CowInteraction prefab)
        {
            if (IsOccupied)
                return _cow;

            if (prefab == null || _spawnPoint == null)
                return null;

            _cow = Instantiate(
                prefab,
                _spawnPoint.position,
                _spawnPoint.rotation,
                _spawnPoint);

            return _cow;
        }
    }
}