using System;
using System.Collections;
using UnityEngine;

namespace FloodRescue50.Rescue
{
    [RequireComponent(typeof(SphereCollider))]
    public class RescuePointController : MonoBehaviour
    {
        [SerializeField]
        private RescuePointData data;

        [SerializeField]
        private RescuePointState currentState = RescuePointState.Unknown;

        private SphereCollider discoveryCollider;
        private Coroutine interactionRoutine;
        private bool interactionsAllowed = true;

        public RescuePointData Data => data;

        public RescuePointState CurrentState => currentState;

        public bool IsCompleted =>
            currentState == RescuePointState.Completed;

        public event Action<RescuePointController, RescuePointState>
            StateChanged;

        public event Action<RescuePointController>
            Completed;

        private void Awake()
        {
            if (data == null)
                Debug.LogWarning("Rescue point is missing RescuePointData; interaction is disabled.", this);
            else
                data.ValidateConfiguration();

            discoveryCollider = GetComponent<SphereCollider>();

            discoveryCollider.isTrigger = true;

            if (data != null)
            {
                discoveryCollider.radius =
                    Mathf.Max(1f, data.discoveryRadius);
            }
        }

        private void OnValidate()
        {
            SphereCollider sphere =
                GetComponent<SphereCollider>();

            if (sphere != null)
            {
                sphere.isTrigger = true;

                if (data != null)
                {
                    sphere.radius =
                        Mathf.Max(1f, data.discoveryRadius);
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            Discover();
        }

        public void Discover()
        {
            if (!isActiveAndEnabled || !interactionsAllowed || data == null || currentState != RescuePointState.Unknown)
                return;

            SetState(RescuePointState.Discovered);
        }

        public bool CanInteractFrom(Vector3 playerPosition)
        {
            if (!isActiveAndEnabled || !interactionsAllowed || data == null)
                return false;

            if (currentState == RescuePointState.Completed || currentState == RescuePointState.Active)
                return false;

            float distance =
                Vector3.Distance(playerPosition, transform.position);

            return distance <= data.interactionRange;
        }

        public bool TryBeginInteraction(GameObject interactor)
        {
            if (interactor == null || !interactor.activeInHierarchy || data == null)
                return false;

            if (!CanInteractFrom(interactor.transform.position))
                return false;

            if (interactionRoutine != null)
                return false;

            if (currentState == RescuePointState.Unknown)
            {
                Discover();
            }

            SetState(RescuePointState.Active);
            if (!isActiveAndEnabled || !interactionsAllowed || currentState != RescuePointState.Active)
                return false;

            interactionRoutine =
                StartCoroutine(InteractionRoutine(interactor));

            return true;
        }

        public void CancelInteraction()
        {
            if (interactionRoutine != null)
                StopCoroutine(interactionRoutine);
            interactionRoutine = null;

            if (currentState == RescuePointState.Active)
            {
                SetState(RescuePointState.Discovered);
                Debug.Log($"Rescue cancelled: {data?.poiId}.", this);
            }
        }

        private void OnDisable()
        {
            CancelInteraction();
        }

        public void SetInteractionsAllowed(bool allowed)
        {
            interactionsAllowed = allowed;
            if (!allowed)
                CancelInteraction();
        }

        private IEnumerator InteractionRoutine(
            GameObject interactor)
        {
            float elapsed = 0f;

            float duration = data.interactionDuration;
            while (elapsed < duration)
            {
                // Check range after yielding, including the final interaction frame.
                yield return null;
                if (!isActiveAndEnabled || !interactionsAllowed || currentState != RescuePointState.Active)
                    yield break;
                if (interactor == null || !interactor.activeInHierarchy || data == null)
                {
                    interactionRoutine = null;
                    CancelInteraction();
                    yield break;
                }

                float distance = Vector3.Distance(
                    interactor.transform.position,
                    transform.position);

                if (distance > data.interactionRange)
                {
                    interactionRoutine = null;
                    CancelInteraction();
                    yield break;
                }

                elapsed += Time.deltaTime;

            }

            interactionRoutine = null;

            Complete();
        }

        private void Complete()
        {
            if (currentState == RescuePointState.Completed)
                return;

            SetState(RescuePointState.Completed);

            Completed?.Invoke(this);
        }

        private void SetState(RescuePointState newState)
        {
            if (currentState == newState)
                return;

            currentState = newState;

            Debug.Log($"POI {data?.poiId}: {currentState}.", this);

            StateChanged?.Invoke(this, currentState);
        }
    }
}
