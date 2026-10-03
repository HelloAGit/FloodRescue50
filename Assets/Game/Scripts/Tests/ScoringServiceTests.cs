using NUnit.Framework;
using FloodRescue50.Scoring;

namespace FloodRescue50.Tests
{
    public class ScoringServiceTests
    {
        [Test]
        public void
            CompletionAtMissionStartReturnsMaximumScore()
        {
            int score =
                ScoringService.CalculatePoiScore(
                    100,
                    0f,
                    900f,
                    300);

            Assert.AreEqual(
                400,
                score);
        }

        [Test]
        public void
            CompletionHalfwayReturnsHalfTimeBonus()
        {
            int score =
                ScoringService.CalculatePoiScore(
                    100,
                    450f,
                    900f,
                    300);

            Assert.AreEqual(
                250,
                score);
        }

        [Test]
        public void
            CompletionAtMissionEndReturnsBaseScore()
        {
            int score =
                ScoringService.CalculatePoiScore(
                    100,
                    900f,
                    900f,
                    300);

            Assert.AreEqual(
                100,
                score);
        }

        [Test]
        public void
            CompletionAfterMissionEndDoesNotGoBelowBaseScore()
        {
            int score =
                ScoringService.CalculatePoiScore(
                    100,
                    1200f,
                    900f,
                    300);

            Assert.AreEqual(
                100,
                score);
        }

        [Test]
        public void
            EarlyCompletionScoresMoreThanLateCompletion()
        {
            int early =
                ScoringService.CalculatePoiScore(
                    100,
                    100f,
                    900f);

            int late =
                ScoringService.CalculatePoiScore(
                    100,
                    700f,
                    900f);

            Assert.Greater(
                early,
                late);
        }
    }
}
