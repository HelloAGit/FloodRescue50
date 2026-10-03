using UnityEngine;
using UnityEngine.InputSystem;

namespace FloodRescue50.Player
{
    public class ThirdPersonCameraController : MonoBehaviour
    {
        [SerializeField]
        private Transform target;

        [SerializeField]
        private Vector3 targetOffset =
            new Vector3(0f, 1.6f, 0f);

        [SerializeField]
        private float distance = 5f;

        [SerializeField]
        private float mouseSensitivity = 0.15f;

        [SerializeField]
        private float minPitch = -25f;

        [SerializeField]
        private float maxPitch = 65f;

        private float yaw;
        private float pitch = 15f;

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        private void Start()
        {
            Cursor.lockState =
                CursorLockMode.Locked;

            Cursor.visible = false;
        }

        private void LateUpdate()
        {
            if (target == null ||
                Mouse.current == null)
            {
                return;
            }

            Vector2 mouseDelta =
                Mouse.current.delta.ReadValue();

            yaw +=
                mouseDelta.x * mouseSensitivity;

            pitch -=
                mouseDelta.y * mouseSensitivity;

            pitch =
                Mathf.Clamp(
                    pitch,
                    minPitch,
                    maxPitch);

            Quaternion rotation =
                Quaternion.Euler(
                    pitch,
                    yaw,
                    0f);

            Vector3 focusPoint =
                target.position + targetOffset;

            Vector3 desiredPosition =
                focusPoint -
                rotation * Vector3.forward * distance;

            transform.position = desiredPosition;

            transform.rotation = rotation;
        }
    }
}
