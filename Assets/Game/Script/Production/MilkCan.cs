using MilkMaker.Items;
using UnityEngine;

namespace MilkMaker.Production
{
    [DisallowMultipleComponent]
    public sealed class MilkCan : MonoBehaviour
    {
        [SerializeField] private ItemHolder _holder;
        [SerializeField] private MilkConveyor _conveyor;

        public void Interact()
        {
            if (_holder == null ||
                _conveyor == null ||
                !_holder.HasItem)
            {
                return;
            }

            _conveyor.TrySend(_holder.HeldBucket);
        }
    }
}