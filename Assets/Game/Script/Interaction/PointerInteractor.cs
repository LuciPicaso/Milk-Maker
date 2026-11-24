using UnityEngine;
using UnityEngine.InputSystem;

namespace MilkMaker.Interaction
{
    [DisallowMultipleComponent]
    public sealed class PointerInteractor : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private LayerMask _raycastMask = ~0;

        [SerializeField, Min(0.1f)]
        private float _maxDistance = 2.5f;

        private void LateUpdate()
        {
            Mouse mouse = Mouse.current;

            if (_camera == null || mouse == null)
                return;

            if (Cursor.lockState != CursorLockMode.Locked)
                return;

            if (!mouse.leftButton.wasPressedThisFrame)
                return;

            Ray ray = _camera.ViewportPointToRay(
                new Vector3(0.5f, 0.5f, 0f));

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