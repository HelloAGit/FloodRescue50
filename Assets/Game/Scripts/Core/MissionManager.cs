using System;
using System.Collections.Generic;
using UnityEngine;
using FloodRescue50.Rescue;
using FloodRescue50.Scoring;

namespace FloodRescue50.Core
{
    public class MissionManager : MonoBehaviour
    {
        [SerializeField]
        private MissionTimer missionTimer;

        [SerializeField]
        private ScoringService scoringService;

        [SerializeField]
        private List<RescuePointController>
            rescuePoints =
                new List<RescuePointController>();

        private bool missionFinished;

        public int TotalPoints =>
            rescuePoints.Count;

        public int CompletedPoints =>
            scoringService != null
                ? scoringService.CompletedPoints
                : 0;

        public event Action<
            int,
            int>
            ProgressChanged;

        public event Action<MissionResults>
            MissionCompleted;

        private void Awake()
        {
            foreach (
                RescuePointController point
                in rescuePoints)
            {
                if (point == null)
                    continue;

                point.Completed +=
                    HandleRescuePointCompleted;
            }

            if (missionTimer != null)
            {
                missionTimer.MissionEnded +=
                    HandleTimerEnded;
            }
        }

        private void OnDestroy()
        {
            foreach (
                RescuePointController point
                in rescuePoints)
            {
                if (point == null)
                    continue;

                point.Completed -=
                    HandleRescuePointCompleted;
            }

            if (missionTimer != null)
            {
                missionTimer.MissionEnded -=
                    HandleTimerEnded;
            }
        }

        private void Start()
        {
            if (scoringService != null)
            {
                scoringService.ResetScore();
            }

            ProgressChanged?.Invoke(
                CompletedPoints,
                TotalPoints);
        }

        private void
            HandleRescuePointCompleted(
                RescuePointController point)
        {
            if (missionFinished ||
                point == null ||
                scoringService == null ||
                missionTimer == null)
            {
                return;
            }

            int baseScore =
                point.Data != null
                    ? point.Data.baseScore
                    : 100;

            scoringService.AwardPoiScore(
                baseScore,
                missionTimer.ElapsedTime,
                missionTimer.MissionDuration);

            ProgressChanged?.Invoke(
                CompletedPoints,
                TotalPoints);

            if (CompletedPoints >=
                TotalPoints)
            {
                FinishMission(true);
            }
        }

        private void HandleTimerEnded()
        {
            FinishMission(false);
        }

        private void FinishMission(
            bool allPointsCompleted)
        {
            if (missionFinished)
                return;

            missionFinished = true;

            if (missionTimer != null)
            {
                missionTimer.StopMission();
            }

            MissionResults results =
                new MissionResults(
                    scoringService != null
                        ? scoringService.TotalScore
                        : 0,
                    CompletedPoints,
                    TotalPoints,
                    missionTimer != null
                        ? missionTimer.ElapsedTime
                        : 0f,
                    allPointsCompleted);

            MissionCompleted?.Invoke(
                results);
        }
    }
}
