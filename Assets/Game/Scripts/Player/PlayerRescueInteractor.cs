using System;
using UnityEngine;
using UnityEngine.InputSystem;
using FloodRescue50.Rescue;

namespace FloodRescue50.Player
{
    public class PlayerRescueInteractor : MonoBehaviour
    {
        [SerializeField]
        private float searchRadius = 4f;

        [SerializeField]
        private LayerMask rescuePointLayers = ~0;

        private RescuePointController currentTarget;

        private void OnDisable()
        {
            currentTarget = null;
            PromptChanged?.Invoke(string.Empty, false);
        }

        public event Action<string, bool>
            PromptChanged;

        private void Update()
        {
            UpdateTarget();

            if (Keyboard.current != null &&
                Keyboard.current.eKey.wasPressedThisFrame)
            {
                TryInteract();
            }
        }

        private void UpdateTarget()
        {
            RescuePointController newTarget =
                FindClosestRescuePoint();

            if (newTarget == currentTarget)
                return;

            currentTarget = newTarget;

            if (currentTarget == null)
            {
                PromptChanged?.Invoke(
                    string.Empty,
                    false);

                return;
            }

            string locationName =
                currentTarget.Data != null
                    ? currentTarget.Data.displayName
                    : "Rescue Point";

            PromptChanged?.Invoke(
                $"[E] Start rescue: {locationName}",
                true);
        }

        private RescuePointController
            FindClosestRescuePoint()
        {
            Collider[] hits =
                Physics.OverlapSphere(
                    transform.position,
                    searchRadius,
                    rescuePointLayers,
                    QueryTriggerInteraction.Collide);

            RescuePointController closest = null;

            float closestDistance =
                float.MaxValue;

            foreach (Collider hit in hits)
            {
                RescuePointController point =
                    hit.GetComponentInParent<
                        RescuePointController>();

                if (point == null)
                    continue;

                if (!point.CanInteractFrom(
                        transform.position))
                {
                    continue;
                }

                float distance =
                    Vector3.Distance(
                        transform.position,
                        point.transform.position);

                if (distance >= closestDistance)
                    continue;

                closestDistance = distance;
                closest = point;
            }

            return closest;
        }

        private void TryInteract()
        {
            if (currentTarget == null)
                return;

            currentTarget.TryBeginInteraction(
                gameObject);
        }
    }
}
