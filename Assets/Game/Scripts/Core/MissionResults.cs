namespace FloodRescue50.Core
{
    public class MissionResults
    {
        public int FinalScore { get; }

        public int CompletedPoints { get; }

        public int TotalPoints { get; }

        public float ElapsedTime { get; }

        public bool AllPointsCompleted { get; }

        public MissionResults(
            int finalScore,
            int completedPoints,
            int totalPoints,
            float elapsedTime,
            bool allPointsCompleted)
        {
            FinalScore = finalScore;

            CompletedPoints =
                completedPoints;

            TotalPoints =
                totalPoints;

            ElapsedTime =
                elapsedTime;

            AllPointsCompleted =
                allPointsCompleted;
        }
    }
}
