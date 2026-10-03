using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FloodRescue50.Rescue;

namespace FloodRescue50.Tests
{
    public class RescuePointControllerTests
    {
        private GameObject host;
        private GameObject rescuer;
        private RescuePointData data;
        private RescuePointController point;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            data = ScriptableObject.CreateInstance<RescuePointData>();
            data.poiId = "TEST";
            data.interactionDuration = 0.1f;
            host = new GameObject("Test POI");
            host.SetActive(false);
            point = host.AddComponent<RescuePointController>();
            typeof(RescuePointController).GetField("data", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(point, data);
            rescuer = new GameObject("Test rescuer");
            host.SetActive(true);
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Object.Destroy(host);
            Object.Destroy(rescuer);
            Object.Destroy(data);
            yield return null;
        }

        [UnityTest]
        public IEnumerator FullLifecycleCompletesOnceAndCannotRestart()
        {
            var states = new List<RescuePointState>();
            int completions = 0;
            point.StateChanged += (sender, state) => states.Add(state);
            point.Completed += sender => completions++;
            Assert.AreEqual(RescuePointState.Unknown, point.CurrentState);
            point.Discover();
            point.Discover();
            Assert.IsTrue(point.TryBeginInteraction(rescuer));
            Assert.AreEqual(RescuePointState.Active, point.CurrentState);
            Assert.IsFalse(point.TryBeginInteraction(rescuer));
            float deadline = Time.realtimeSinceStartup + 5f;
            while (!point.IsCompleted && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.IsTrue(point.IsCompleted);
            Assert.AreEqual(1, completions);
            CollectionAssert.AreEqual(new[] { RescuePointState.Discovered, RescuePointState.Active, RescuePointState.Completed }, states);
            Assert.IsFalse(point.TryBeginInteraction(rescuer));
            point.Discover();
            point.CancelInteraction();
            Assert.AreEqual(RescuePointState.Completed, point.CurrentState);
            Assert.AreEqual(1, completions);
        }

        [UnityTest]
        public IEnumerator LeavingRangeCancelsAndAllowsRetry()
        {
            data.interactionDuration = 5f;
            Assert.IsTrue(point.TryBeginInteraction(rescuer));
            rescuer.transform.position = Vector3.right * 20f;
            yield return null;
            yield return null;
            Assert.AreEqual(RescuePointState.Discovered, point.CurrentState);
            rescuer.transform.position = Vector3.zero;
            Assert.IsTrue(point.TryBeginInteraction(rescuer));
        }

        [UnityTest]
        public IEnumerator DisablingPointCancelsWithoutCompleting()
        {
            Assert.IsTrue(point.TryBeginInteraction(rescuer));
            point.enabled = false;
            yield return null;
            Assert.AreEqual(RescuePointState.Discovered, point.CurrentState);
            Assert.IsFalse(point.TryBeginInteraction(rescuer));
            point.enabled = true;
            Assert.IsTrue(point.TryBeginInteraction(rescuer));
        }

        [Test]
        public void MissingDataFailsSafely()
        {
            typeof(RescuePointController).GetField("data", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(point, null);
            Assert.IsFalse(point.CanInteractFrom(Vector3.zero));
            Assert.IsFalse(point.TryBeginInteraction(rescuer));
            point.Discover();
            Assert.AreEqual(RescuePointState.Unknown, point.CurrentState);
        }

        [Test]
        public void DisabledMissionPointCannotInteract()
        {
            point.SetInteractionsAllowed(false);
            Assert.IsFalse(point.TryBeginInteraction(rescuer));
            point.Discover();
            Assert.AreEqual(RescuePointState.Unknown, point.CurrentState);
        }
    }
}
