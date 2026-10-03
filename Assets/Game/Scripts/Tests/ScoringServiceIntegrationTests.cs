using NUnit.Framework;
using UnityEngine;
using FloodRescue50.Scoring;

namespace FloodRescue50.Tests
{
    public class ScoringServiceIntegrationTests
    {
        private GameObject host;
        private ScoringService scoring;

        [SetUp]
        public void SetUp()
        {
            host = new GameObject("Scoring test");
            scoring = host.AddComponent<ScoringService>();
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(host);

        [TestCase(-20f, 900f, 100, 300, 400)]
        [TestCase(0f, 0f, 100, 300, 100)]
        [TestCase(0f, -1f, 100, 300, 100)]
        [TestCase(0f, 900f, -100, 300, 300)]
        [TestCase(900f, 900f, -100, 300, 0)]
        [TestCase(0f, 900f, 100, -300, 100)]
        [TestCase(float.NaN, 900f, 100, 300, 100)]
        [TestCase(float.PositiveInfinity, 900f, 100, 300, 100)]
        public void InvalidInputsHaveSafeScores(float elapsed, float duration, int baseScore,
            int bonus, int expected)
        {
            Assert.AreEqual(expected, ScoringService.CalculatePoiScore(baseScore, elapsed, duration, bonus));
        }

        [Test]
        public void MultipleAwardsUpdateTotalsAndEvents()
        {
            int scoreEvents = 0;
            int poiEvents = 0;
            scoring.ScoreChanged += score => { scoreEvents++; Assert.AreEqual(scoring.TotalScore, score); };
            scoring.PoiScored += (earned, total) => { poiEvents++; Assert.AreEqual(scoring.TotalScore, total); };
            Assert.AreEqual(400, scoring.AwardPoiScore(100, 0f, 900f));
            Assert.AreEqual(250, scoring.AwardPoiScore(100, 450f, 900f));
            Assert.AreEqual(650, scoring.TotalScore);
            Assert.AreEqual(2, scoring.CompletedPoints);
            Assert.AreEqual(250, scoring.LatestPoiScore);
            Assert.AreEqual(2, scoreEvents);
            Assert.AreEqual(2, poiEvents);
        }

        [Test]
        public void DuplicateIdCannotScoreOrEmitEventsAgain()
        {
            int events = 0;
            scoring.PoiScored += (earned, total) => events++;
            Assert.IsTrue(scoring.TryAwardPoiScore("POI_001", 100, 0f, 900f, out int first));
            Assert.IsFalse(scoring.TryAwardPoiScore(" POI_001 ", 100, 450f, 900f, out int duplicate));
            Assert.AreEqual(400, first);
            Assert.AreEqual(0, duplicate);
            Assert.AreEqual(400, scoring.TotalScore);
            Assert.AreEqual(1, scoring.CompletedPoints);
            Assert.AreEqual(1, events);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void EmptyIdDoesNotAwardScore(string id)
        {
            Assert.IsFalse(scoring.TryAwardPoiScore(id, 100, 0f, 900f, out _));
            Assert.AreEqual(0, scoring.CompletedPoints);
        }

        [Test]
        public void ResetClearsTotalsAndAllowsIdInNewMission()
        {
            scoring.TryAwardPoiScore("POI_001", 100, 0f, 900f, out _);
            scoring.ResetScore();
            Assert.AreEqual(0, scoring.TotalScore);
            Assert.AreEqual(0, scoring.CompletedPoints);
            Assert.AreEqual(0, scoring.LatestPoiScore);
            Assert.IsTrue(scoring.TryAwardPoiScore("POI_001", 100, 900f, 900f, out _));
        }
    }
}
