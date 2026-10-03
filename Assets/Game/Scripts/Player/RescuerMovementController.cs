using UnityEngine;
using UnityEngine.InputSystem;

namespace FloodRescue50.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class RescuerMovementController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField]
        private float walkSpeed = 4f;

        [SerializeField]
        private float sprintSpeed = 7f;

        [SerializeField]
        private float rotationSpeed = 12f;

        [Header("Jump")]
        [SerializeField]
        private float jumpHeight = 1.2f;

        [SerializeField]
        private float gravity = -20f;

        [Header("References")]
        [SerializeField]
        private Transform cameraTransform;

        private CharacterController controller;

        private float verticalVelocity;

        private void Awake()
        {
            controller =
                GetComponent<CharacterController>();
        }

        private void Start()
        {
            if (cameraTransform == null &&
                Camera.main != null)
            {
                cameraTransform =
                    Camera.main.transform;
            }
        }

        private void Update()
        {
            HandleMovement();
        }

        private void HandleMovement()
        {
            if (Keyboard.current == null)
                return;

            Vector2 input = Vector2.zero;

            if (Keyboard.current.wKey.isPressed)
                input.y += 1f;

            if (Keyboard.current.sKey.isPressed)
                input.y -= 1f;

            if (Keyboard.current.dKey.isPressed)
                input.x += 1f;

            if (Keyboard.current.aKey.isPressed)
                input.x -= 1f;

            input = Vector2.ClampMagnitude(input, 1f);

            Vector3 forward =
                cameraTransform != null
                    ? cameraTransform.forward
                    : Vector3.forward;

            Vector3 right =
                cameraTransform != null
                    ? cameraTransform.right
                    : Vector3.right;

            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();

            Vector3 moveDirection =
                forward * input.y +
                right * input.x;

            bool sprinting =
                Keyboard.current.leftShiftKey.isPressed;

            float speed =
                sprinting ? sprintSpeed : walkSpeed;

            if (moveDirection.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation =
                    Quaternion.LookRotation(moveDirection);

                transform.rotation =
                    Quaternion.Slerp(
                        transform.rotation,
                        targetRotation,
                        rotationSpeed * Time.deltaTime);
            }

            HandleGravityAndJump();

            Vector3 velocity =
                moveDirection * speed;

            velocity.y = verticalVelocity;

            controller.Move(
                velocity * Time.deltaTime);
        }

        private void HandleGravityAndJump()
        {
            if (controller.isGrounded &&
                verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            if (controller.isGrounded &&
                Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                verticalVelocity =
                    Mathf.Sqrt(
                        jumpHeight * -2f * gravity);
            }

            verticalVelocity +=
                gravity * Time.deltaTime;
        }
    }
}
