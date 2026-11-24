using UnityEngine;
using UnityEngine.InputSystem;

namespace MilkMaker.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public sealed class FirstPersonController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform _cameraPivot;

        [Header("Movement")]
        [SerializeField, Min(0f)]
        private float _moveSpeed = 4.5f;

        [SerializeField]
        private float _gravity = -20f;

        [Header("Look")]
        [SerializeField, Min(0f)]
        private float _sensitivity = 0.12f;

        [SerializeField, Range(0f, 89f)]
        private float _pitchLimit = 85f;

        private CharacterController _controller;

        private float _pitch;
        private float _verticalVelocity;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();

            if (_cameraPivot == null)
            {
                Debug.LogError(
                    "[FirstPersonController] CameraPivot is missing.",
                    this);

                enabled = false;
                return;
            }

            _pitch = Mathf.DeltaAngle(
                0f,
                _cameraPivot.localEulerAngles.x);
        }

        private void Start()
        {
            SetCursorLocked(true);
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            Mouse mouse = Mouse.current;

            if (keyboard != null &&
                keyboard.escapeKey.wasPressedThisFrame)
            {
                SetCursorLocked(
                    Cursor.lockState != CursorLockMode.Locked);

                return;
            }

            bool hasControl =
                Cursor.lockState == CursorLockMode.Locked;

            if (hasControl && mouse != null)
                UpdateLook(mouse.delta.ReadValue());

            Vector2 input = hasControl && keyboard != null
                ? ReadMovement(keyboard)
                : Vector2.zero;

            UpdateMovement(input);
        }

        private void UpdateLook(Vector2 delta)
        {
            float yaw = delta.x * _sensitivity;

            _pitch -= delta.y * _sensitivity;

            _pitch = Mathf.Clamp(
                _pitch,
                -_pitchLimit,
                _pitchLimit);

            transform.Rotate(
                Vector3.up,
                yaw,
                Space.World);

            _cameraPivot.localRotation =
                Quaternion.Euler(_pitch, 0f, 0f);
        }

        private void UpdateMovement(Vector2 input)
        {
            Vector3 direction =
                transform.right * input.x +
                transform.forward * input.y;

            if (_controller.isGrounded &&
                _verticalVelocity < 0f)
            {
                _verticalVelocity = -2f;
            }

            _verticalVelocity +=
                _gravity * Time.deltaTime;

            Vector3 velocity = direction * _moveSpeed;
            velocity.y = _verticalVelocity;

            _controller.Move(
                velocity * Time.deltaTime);
        }

        private static Vector2 ReadMovement(
            Keyboard keyboard)
        {
            float x = 0f;
            float y = 0f;

            if (keyboard.aKey.isPressed) x -= 1f;
            if (keyboard.dKey.isPressed) x += 1f;
            if (keyboard.sKey.isPressed) y -= 1f;
            if (keyboard.wKey.isPressed) y += 1f;

            return Vector2.ClampMagnitude(
                new Vector2(x, y),
                1f);
        }

        private static void SetCursorLocked(bool locked)
        {
            Cursor.lockState = locked
                ? CursorLockMode.Locked
                : CursorLockMode.None;

            Cursor.visible = !locked;
        }

        private void OnDisable()
        {
            SetCursorLocked(false);
        }
    }
}