using System;
using System.Collections.Generic;
using UnityEngine;

namespace FloodRescue50.Scoring
{
    public class ScoringService : MonoBehaviour
    {
        [SerializeField]
        private int maxTimeBonus = 300;

        private readonly HashSet<string> scoredPoiIds = new HashSet<string>(StringComparer.Ordinal);

        public int TotalScore { get; private set; }

        public int CompletedPoints { get; private set; }

        public int LatestPoiScore { get; private set; }

        public event Action<int>
            ScoreChanged;

        public event Action<int, int>
            PoiScored;

        public void ResetScore()
        {
            scoredPoiIds.Clear();
            TotalScore = 0;
            CompletedPoints = 0;
            LatestPoiScore = 0;

            ScoreChanged?.Invoke(
                TotalScore);
        }

        public int AwardPoiScore(
            int baseScore,
            float elapsedTime,
            float missionDuration)
        {
            int earnedScore =
                CalculatePoiScore(
                    baseScore,
                    elapsedTime,
                    missionDuration,
                    maxTimeBonus);

            LatestPoiScore = earnedScore;

            TotalScore += earnedScore;

            CompletedPoints++;

            ScoreChanged?.Invoke(
                TotalScore);

            PoiScored?.Invoke(
                earnedScore,
                TotalScore);

            return earnedScore;
        }

        public bool TryAwardPoiScore(string poiId, int baseScore, float elapsedTime,
            float missionDuration, out int earnedScore)
        {
            earnedScore = 0;
            if (string.IsNullOrWhiteSpace(poiId) || !scoredPoiIds.Add(poiId.Trim()))
                return false;

            earnedScore = AwardPoiScore(baseScore, elapsedTime, missionDuration);
            Debug.Log($"Score awarded: {poiId}, {earnedScore} points.", this);
            return true;
        }

        public static int CalculatePoiScore(
            int baseScore,
            float elapsedTime,
            float missionDuration,
            int maxTimeBonus = 300)
        {
            baseScore = Mathf.Max(0, baseScore);
            maxTimeBonus = Mathf.Max(0, maxTimeBonus);
            if (missionDuration <= 0f || float.IsNaN(missionDuration) || float.IsInfinity(missionDuration))
            {
                return Mathf.Max(
                    0,
                    baseScore);
            }

            float normalizedTime =
                Mathf.Clamp01(
                    (float.IsNaN(elapsedTime) ? missionDuration : elapsedTime) /
                    missionDuration);

            float timeBonus =
                maxTimeBonus *
                (1f - normalizedTime);

            return Mathf.RoundToInt(
                Mathf.Max(
                    0f,
                    baseScore + timeBonus));
        }
    }
}
