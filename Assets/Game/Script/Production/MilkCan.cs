using MilkMaker.Items;
using UnityEngine;
using UnityEngine.Events;

namespace MilkMaker.Production
{
    [DisallowMultipleComponent]
    public sealed class MilkCan : MonoBehaviour
    {
        [Header("Presentation")]
        [SerializeField] private UnityEvent _onPoured = new();

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

            if (_conveyor.TrySend(_holder.HeldBucket))
            {
                _onPoured.Invoke();
            }
        }
    }
}