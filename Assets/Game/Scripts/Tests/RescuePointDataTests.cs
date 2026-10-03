using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FloodRescue50.Rescue;

namespace FloodRescue50.Tests
{
    public class RescuePointDataTests
    {
        [Test]
        public void InvalidConfigurationUsesSafeValues()
        {
            var data = ScriptableObject.CreateInstance<RescuePointData>();
            try
            {
                data.poiId = "TEST";
                data.numberOfVictims = -3;
                data.criticalVictims = 7;
                data.baseScore = -1;
                data.discoveryRadius = float.NaN;
                data.interactionRange = -1;
                data.interactionDuration = float.PositiveInfinity;
                LogAssert.Expect(LogType.Warning, "Invalid rescue configuration; applying safe victim, score and interaction limits.");
                data.ValidateConfiguration();
                Assert.AreEqual(0, data.numberOfVictims);
                Assert.AreEqual(0, data.criticalVictims);
                Assert.AreEqual(0, data.baseScore);
                Assert.AreEqual(12f, data.discoveryRadius);
                Assert.AreEqual(3f, data.interactionRange);
                Assert.AreEqual(3f, data.interactionDuration);
            }
            finally { Object.DestroyImmediate(data); }
        }

        [Test]
        public void EmptyIdWarnsAndCriticalCountAndDiscoveryRangeAreClamped()
        {
            var data = ScriptableObject.CreateInstance<RescuePointData>();
            try
            {
                data.poiId = " ";
                data.numberOfVictims = 2;
                data.criticalVictims = 3;
                data.discoveryRadius = 1f;
                data.interactionRange = 5f;
                LogAssert.Expect(LogType.Warning, "RescuePointData has an empty POI ID; mission uses point identity as fallback.");
                LogAssert.Expect(LogType.Warning, "Invalid rescue configuration; applying safe victim, score and interaction limits.");
                data.ValidateConfiguration();
                Assert.AreEqual(2, data.criticalVictims);
                Assert.AreEqual(5f, data.discoveryRadius);
            }
            finally { Object.DestroyImmediate(data); }
        }
    }
}
