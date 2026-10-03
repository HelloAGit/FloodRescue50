using System;
using System.Collections.Generic;
using UnityEngine;
using FloodRescue50.Rescue;
using FloodRescue50.Scoring;

namespace FloodRescue50.Core
{
    public class MissionManager : MonoBehaviour
    {
        [SerializeField] private MissionTimer missionTimer;
        [SerializeField] private ScoringService scoringService;
        [SerializeField] private List<RescuePointController> rescuePoints = new List<RescuePointController>();

        private readonly Dictionary<RescuePointController, string> configuredPoints =
            new Dictionary<RescuePointController, string>();
        private readonly HashSet<RescuePointController> completedPoints = new HashSet<RescuePointController>();
        private bool missionFinished;

        public int TotalPoints => configuredPoints.Count;
        public int CompletedPoints => completedPoints.Count;
        public event Action<int, int> ProgressChanged;
        public event Action<MissionResults> MissionCompleted;

        private void Awake()
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (RescuePointController point in rescuePoints)
            {
                if (point == null)
                {
                    Debug.LogWarning("Mission configuration contains a missing rescue point; skipping it.", this);
                    continue;
                }
                if (configuredPoints.ContainsKey(point))
                {
                    Debug.LogWarning("Duplicate rescue point reference; counting it only once.", this);
                    continue;
                }
                if (point.Data == null)
                {
                    Debug.LogWarning("Mission point has no RescuePointData; skipping it.", point);
                    point.SetInteractionsAllowed(false);
                    continue;
                }

                point.Data.ValidateConfiguration();
                string id = point.Data.poiId?.Trim();
                id = string.IsNullOrWhiteSpace(id) ? "point:" + point.GetInstanceID() : "poi:" + id;
                if (!ids.Add(id))
                {
                    Debug.LogWarning($"Duplicate POI ID {point.Data.poiId}; disabling the duplicate point.", point);
                    point.SetInteractionsAllowed(false);
                    continue;
                }
                configuredPoints.Add(point, id);
                point.Completed += HandleRescuePointCompleted;
            }
            if (missionTimer != null)
                missionTimer.MissionEnded += HandleTimerEnded;
        }

        private void Start()
        {
            if (scoringService == null || missionTimer == null)
            {
                Debug.LogWarning("Mission requires MissionTimer and ScoringService; gameplay is disabled.", this);
                foreach (RescuePointController point in configuredPoints.Keys)
                    point.SetInteractionsAllowed(false);
                if (missionTimer != null) missionTimer.StopMission();
                return;
            }
            if (TotalPoints == 0)
                Debug.LogWarning("Mission has zero valid rescue points; it will end only when the timer expires.", this);
            scoringService.ResetScore();
            ProgressChanged?.Invoke(CompletedPoints, TotalPoints);
            missionTimer.StartMission();
        }

        private void OnDestroy()
        {
            foreach (RescuePointController point in configuredPoints.Keys)
                if (point != null)
                    point.Completed -= HandleRescuePointCompleted;
            if (missionTimer != null)
                missionTimer.MissionEnded -= HandleTimerEnded;
        }

        private void HandleRescuePointCompleted(RescuePointController point)
        {
            if (missionFinished || point == null || !point.IsCompleted ||
                scoringService == null || missionTimer == null || !missionTimer.IsRunning ||
                !configuredPoints.TryGetValue(point, out string id) || completedPoints.Contains(point))
                return;

            // Capture identity at setup, independent of scene names or later data edits.
            if (!scoringService.TryAwardPoiScore(id, point.Data.baseScore,
                missionTimer.ElapsedTime, missionTimer.MissionDuration, out _))
                return;
            completedPoints.Add(point);
            ProgressChanged?.Invoke(CompletedPoints, TotalPoints);
            if (TotalPoints > 0 && CompletedPoints == TotalPoints)
                FinishMission(true);
        }

        private void HandleTimerEnded()
        {
            FinishMission(false);
        }

        private void FinishMission(bool allPointsCompleted)
        {
            if (missionFinished) return;
            missionFinished = true;
            if (missionTimer != null) missionTimer.StopMission();
            foreach (RescuePointController point in configuredPoints.Keys)
                if (point != null)
                    point.SetInteractionsAllowed(false);
            var results = new MissionResults(scoringService != null ? scoringService.TotalScore : 0,
                CompletedPoints, TotalPoints, missionTimer != null ? missionTimer.ElapsedTime : 0f,
                allPointsCompleted);
            Debug.Log(allPointsCompleted ? "Mission completed: all rescue points cleared." : "Mission ended: time expired.", this);
            MissionCompleted?.Invoke(results);
        }
    }
}
