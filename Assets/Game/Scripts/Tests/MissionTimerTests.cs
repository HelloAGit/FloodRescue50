using NUnit.Framework;
using UnityEngine;
using FloodRescue50.Core;

namespace FloodRescue50.Tests
{
    public class MissionTimerTests
    {
        private GameObject host;
        private MissionTimer timer;

        [SetUp]
        public void SetUp()
        {
            host = new GameObject("Timer test");
            timer = host.AddComponent<MissionTimer>();
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(host);

        [Test]
        public void DefaultTimerHasFifteenMinutesAndWaitsForMissionStart()
        {
            Assert.AreEqual(900f, timer.MissionDuration);
            Assert.AreEqual(900f, timer.RemainingTime);
            Assert.AreEqual(0f, timer.ElapsedTime);
            Assert.AreEqual(0f, timer.NormalizedElapsedTime);
            Assert.IsFalse(timer.IsRunning);
            timer.AdvanceTime(100f);
            Assert.AreEqual(0f, timer.ElapsedTime);
        }

        [Test]
        public void StartPublishesStartAndInitialRemainingTime()
        {
            int starts = 0;
            float remaining = -1f;
            timer.MissionStarted += () => starts++;
            timer.TimerChanged += value => remaining = value;
            timer.StartMission();
            Assert.AreEqual(1, starts);
            Assert.AreEqual(900f, remaining);
            Assert.IsTrue(timer.IsRunning);
        }

        [TestCase(0f, 900f, 0f)]
        [TestCase(450f, 450f, 0.5f)]
        [TestCase(900f, 0f, 1f)]
        [TestCase(1200f, 0f, 1f)]
        public void AdvanceClampsRemainingAndNormalizedTime(float delta, float remaining, float normalized)
        {
            timer.StartMission();
            timer.AdvanceTime(delta);
            Assert.AreEqual(remaining, timer.RemainingTime);
            Assert.AreEqual(normalized, timer.NormalizedElapsedTime);
            Assert.LessOrEqual(timer.ElapsedTime, timer.MissionDuration);
        }

        [Test]
        public void ExpiryPublishesZeroThenEndsExactlyOnce()
        {
            int ends = 0;
            float lastRemaining = -1f;
            timer.TimerChanged += value => lastRemaining = value;
            timer.MissionEnded += () => { ends++; Assert.AreEqual(0f, lastRemaining); };
            timer.StartMission();
            timer.AdvanceTime(1000f);
            timer.AdvanceTime(1000f);
            timer.StopMission();
            Assert.AreEqual(1, ends);
            Assert.IsFalse(timer.IsRunning);
        }

        [Test]
        public void StopDoesNotExpireAndRestartResetsElapsedTime()
        {
            int ends = 0;
            timer.MissionEnded += () => ends++;
            timer.StartMission();
            timer.AdvanceTime(50f);
            timer.StopMission();
            timer.AdvanceTime(1000f);
            Assert.AreEqual(50f, timer.ElapsedTime);
            Assert.AreEqual(0, ends);
            timer.StartMission();
            Assert.AreEqual(0f, timer.ElapsedTime);
            Assert.AreEqual(900f, timer.RemainingTime);
            timer.AdvanceTime(900f);
            Assert.AreEqual(1, ends);
        }

        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void InvalidTimeStepIsIgnored(float delta)
        {
            timer.StartMission();
            timer.AdvanceTime(delta);
            Assert.AreEqual(0f, timer.ElapsedTime);
            Assert.IsTrue(timer.IsRunning);
        }
    }
}
