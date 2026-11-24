using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace MilkMaker.Interaction
{
    [DisallowMultipleComponent]
    public sealed class PointerInteractor : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private LayerMask _raycastMask = ~0;
        [SerializeField, Min(0.1f)]
        private float _maxDistance = 100f;

        private void LateUpdate()
        {
            Mouse mouse = Mouse.current;

            if (_camera == null || mouse == null)
                return;

            if (!mouse.leftButton.wasPressedThisFrame)
                return;

            if (EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject(
                    mouse.deviceId))
            {
                return;
            }

            Ray ray = _camera.ScreenPointToRay(
                mouse.position.ReadValue());

            if (!Physics.Raycast(
                    ray,
                    out RaycastHit hit,
                    _maxDistance,
                    _raycastMask,
                    QueryTriggerInteraction.Collide))
            {
                return;
            }

            if (hit.collider.TryGetComponent(
                    out InteractionTarget target))
            {
                target.Interact();
            }
        }
    }
}