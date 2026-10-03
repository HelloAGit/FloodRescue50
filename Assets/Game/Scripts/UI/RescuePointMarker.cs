using TMPro;
using UnityEngine;
using FloodRescue50.Rescue;

namespace FloodRescue50.UI
{
    public class RescuePointMarker : MonoBehaviour
    {
        [SerializeField] private RescuePointController rescuePoint;
        [SerializeField] private TMP_Text markerText;
        private Camera viewCamera;

        private void OnEnable()
        {
            if (rescuePoint != null)
            {
                rescuePoint.StateChanged += UpdateState;
                UpdateState(rescuePoint, rescuePoint.CurrentState);
            }
        }

        private void OnDisable()
        {
            if (rescuePoint != null)
                rescuePoint.StateChanged -= UpdateState;
        }

        private void LateUpdate()
        {
            if (viewCamera == null) viewCamera = Camera.main;
            if (viewCamera != null) transform.rotation = viewCamera.transform.rotation;
        }

        private void UpdateState(RescuePointController point, RescuePointState state)
        {
            if (markerText == null) return;
            markerText.enabled = state != RescuePointState.Unknown;
            markerText.text = state == RescuePointState.Completed ? "\u2713" :
                state == RescuePointState.Active ? "!" : "?";
            markerText.color = state == RescuePointState.Completed ? Color.green :
                state == RescuePointState.Active ? Color.yellow : Color.white;
        }
    }
}
