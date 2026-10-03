using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FloodRescue50.Core;
using FloodRescue50.Rescue;
using FloodRescue50.Scoring;

namespace FloodRescue50.Tests
{
    public class MissionManagerTests
    {
        private GameObject host;
        private GameObject rescuer;
        private MissionManager mission;
        private MissionTimer timer;
        private ScoringService scoring;
        private readonly List<RescuePointData> data = new List<RescuePointData>();
        private readonly List<RescuePointController> points = new List<RescuePointController>();

        private void CreateMission(int count, bool duplicateReference = false, bool duplicateId = false)
        {
            host = new GameObject("Mission test");
            host.SetActive(false);
            timer = host.AddComponent<MissionTimer>();
            scoring = host.AddComponent<ScoringService>();
            mission = host.AddComponent<MissionManager>();
            for (int i = 0; i < count; i++)
            {
                var definition = ScriptableObject.CreateInstance<RescuePointData>();
                definition.poiId = duplicateId ? "POI_001" : "POI_" + i;
                definition.interactionDuration = 0.1f;
                data.Add(definition);
                var pointObject = new GameObject("POI " + i);
                pointObject.transform.SetParent(host.transform);
                var point = pointObject.AddComponent<RescuePointController>();
                SetField(point, "data", definition);
                points.Add(point);
            }
            var configured = new List<RescuePointController>(points);
            if (duplicateReference) configured.Add(points[0]);
            SetField(mission, "rescuePoints", configured);
            SetField(mission, "missionTimer", timer);
            SetField(mission, "scoringService", scoring);
            rescuer = new GameObject("Rescuer");
        }

        private static void SetField(object instance, string name, object value)
        {
            instance.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(instance, value);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Object.Destroy(host);
            Object.Destroy(rescuer);
            foreach (var definition in data) Object.Destroy(definition);
            data.Clear();
            points.Clear();
            yield return null;
        }

        [UnityTest]
        public IEnumerator AllFivePointsFinishOnceAndStopTimer()
        {
            CreateMission(5);
            int finishes = 0;
            MissionResults results = null;
            mission.MissionCompleted += value => { finishes++; results = value; };
            host.SetActive(true);
            yield return null;
            // Advance explicitly so this test does not depend on test-runner frame speed.
            timer.enabled = false;
            foreach (var point in points)
            {
                Assert.IsTrue(point.TryBeginInteraction(rescuer));
                float deadline = Time.realtimeSinceStartup + 5f;
                while (!point.IsCompleted && Time.realtimeSinceStartup < deadline) yield return null;
                Assert.IsTrue(point.IsCompleted);
            }
            Assert.AreEqual(1, finishes);
            Assert.IsTrue(results.AllPointsCompleted);
            Assert.AreEqual(5, results.CompletedPoints);
            Assert.AreEqual(5, results.TotalPoints);
            Assert.Greater(results.FinalScore, 500);
            Assert.IsFalse(timer.IsRunning);
            timer.AdvanceTime(900f);
            Assert.AreEqual(1, finishes);
            Assert.AreEqual(5, scoring.CompletedPoints);
        }

        [UnityTest]
        public IEnumerator ExpiryCancelsActiveRescueAndPreventsLateScore()
        {
            CreateMission(1);
            data[0].interactionDuration = 5f;
            int finishes = 0;
            MissionResults results = null;
            mission.MissionCompleted += value => { finishes++; results = value; };
            host.SetActive(true);
            yield return null;
            Assert.IsTrue(points[0].TryBeginInteraction(rescuer));
            timer.AdvanceTime(900f);
            yield return null;
            Assert.AreEqual(1, finishes);
            Assert.IsFalse(results.AllPointsCompleted);
            Assert.AreEqual(0, results.FinalScore);
            Assert.AreEqual(RescuePointState.Discovered, points[0].CurrentState);
            Assert.IsFalse(points[0].TryBeginInteraction(rescuer));
        }

        [UnityTest]
        public IEnumerator DuplicateReferenceCountsOnePoint()
        {
            CreateMission(1, duplicateReference: true);
            LogAssert.Expect(LogType.Warning, "Duplicate rescue point reference; counting it only once.");
            host.SetActive(true);
            yield return null;
            Assert.AreEqual(1, mission.TotalPoints);
            Assert.IsTrue(points[0].TryBeginInteraction(rescuer));
            float deadline = Time.realtimeSinceStartup + 5f;
            while (!points[0].IsCompleted && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.AreEqual(1, scoring.CompletedPoints);
        }

        [UnityTest]
        public IEnumerator DuplicateIdIsDisabledAndExcludedFromTotal()
        {
            CreateMission(2, duplicateId: true);
            LogAssert.Expect(LogType.Warning, "Duplicate POI ID POI_001; disabling the duplicate point.");
            host.SetActive(true);
            yield return null;
            Assert.AreEqual(1, mission.TotalPoints);
            Assert.IsFalse(points[1].TryBeginInteraction(rescuer));
        }

        [UnityTest]
        public IEnumerator EmptyMissionWarnsWithoutImmediateSuccess()
        {
            CreateMission(0);
            int finishes = 0;
            mission.MissionCompleted += results => { finishes++; Assert.IsFalse(results.AllPointsCompleted); };
            LogAssert.Expect(LogType.Warning, "Mission has zero valid rescue points; it will end only when the timer expires.");
            host.SetActive(true);
            yield return null;
            Assert.AreEqual(0, finishes);
            timer.AdvanceTime(900f);
            Assert.AreEqual(1, finishes);
        }
    }
}
