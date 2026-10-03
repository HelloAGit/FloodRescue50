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
            if (currentState != RescuePointState.Unknown)
                return;

            SetState(RescuePointState.Discovered);
        }

        public bool CanInteractFrom(Vector3 playerPosition)
        {
            if (data == null)
                return false;

            if (currentState == RescuePointState.Completed)
                return false;

            float distance =
                Vector3.Distance(playerPosition, transform.position);

            return distance <= data.interactionRange;
        }

        public bool TryBeginInteraction(GameObject interactor)
        {
            if (interactor == null || data == null)
                return false;

            if (!CanInteractFrom(interactor.transform.position))
                return false;

            if (interactionRoutine != null)
                return false;

            if (currentState == RescuePointState.Unknown)
            {
                Discover();
            }

            interactionRoutine =
                StartCoroutine(InteractionRoutine(interactor));

            return true;
        }

        public void CancelInteraction()
        {
            if (interactionRoutine == null)
                return;

            StopCoroutine(interactionRoutine);
            interactionRoutine = null;

            if (currentState == RescuePointState.Active)
            {
                SetState(RescuePointState.Discovered);
            }
        }

        private IEnumerator InteractionRoutine(
            GameObject interactor)
        {
            SetState(RescuePointState.Active);

            float elapsed = 0f;

            while (elapsed < data.interactionDuration)
            {
                if (interactor == null)
                {
                    CancelInteraction();
                    yield break;
                }

                float distance = Vector3.Distance(
                    interactor.transform.position,
                    transform.position);

                if (distance > data.interactionRange)
                {
                    CancelInteraction();
                    yield break;
                }

                elapsed += Time.deltaTime;

                yield return null;
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

            StateChanged?.Invoke(this, currentState);
        }
    }
}
