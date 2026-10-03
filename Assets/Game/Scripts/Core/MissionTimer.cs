using System;
using UnityEngine;

namespace FloodRescue50.Core
{
    public class MissionTimer : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float missionDuration = 900f;

        public float MissionDuration => missionDuration;
        public float ElapsedTime { get; private set; }
        public float RemainingTime => Mathf.Max(0f, missionDuration - ElapsedTime);
        public float NormalizedElapsedTime => missionDuration > 0f
            ? Mathf.Clamp01(ElapsedTime / missionDuration) : 1f;
        public bool IsRunning { get; private set; }

        public event Action MissionStarted;
        public event Action<float> TimerChanged;
        public event Action MissionEnded;

        private void Update()
        {
            AdvanceTime(Time.deltaTime);
        }

        public void StartMission()
        {
            if (float.IsNaN(missionDuration) || float.IsInfinity(missionDuration) || missionDuration <= 0f)
            {
                Debug.LogWarning("Invalid mission duration; using 900 seconds.", this);
                missionDuration = 900f;
            }

            ElapsedTime = 0f;
            IsRunning = true;
            Debug.Log("Mission started.", this);
            MissionStarted?.Invoke();
            TimerChanged?.Invoke(RemainingTime);
        }

        public void StopMission()
        {
            IsRunning = false;
        }

        // Explicit stepping lets tests or a future authoritative clock drive the timer.
        public void AdvanceTime(float deltaSeconds)
        {
            if (!IsRunning || deltaSeconds <= 0f || float.IsNaN(deltaSeconds) || float.IsInfinity(deltaSeconds))
                return;

            ElapsedTime = Mathf.Min(missionDuration, ElapsedTime + deltaSeconds);
            bool expired = RemainingTime <= 0f;
            if (expired)
                IsRunning = false;

            TimerChanged?.Invoke(RemainingTime);
            if (expired)
            {
                Debug.Log("Mission expired.", this);
                MissionEnded?.Invoke();
            }
        }
    }
}
