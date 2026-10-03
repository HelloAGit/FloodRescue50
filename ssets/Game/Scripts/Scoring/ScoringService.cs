using System;
using UnityEngine;

namespace FloodRescue50.Scoring
{
    public class ScoringService : MonoBehaviour
    {
        [SerializeField]
        private int maxTimeBonus = 300;

        public int TotalScore { get; private set; }

        public int CompletedPoints { get; private set; }

        public int LatestPoiScore { get; private set; }

        public event Action<int>
            ScoreChanged;

        public event Action<int, int>
            PoiScored;

        public void ResetScore()
        {
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

        public static int CalculatePoiScore(
            int baseScore,
            float elapsedTime,
            float missionDuration,
            int maxTimeBonus = 300)
        {
            if (missionDuration <= 0f)
            {
                return Mathf.Max(
                    0,
                    baseScore);
            }

            float normalizedTime =
                Mathf.Clamp01(
                    elapsedTime /
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
