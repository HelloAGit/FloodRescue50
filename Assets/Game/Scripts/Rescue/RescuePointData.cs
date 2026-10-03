using UnityEngine;

namespace FloodRescue50.Rescue
{
    [CreateAssetMenu(
        fileName = "RescuePointData",
        menuName = "Flood Rescue 50/Rescue Point Data")]
    public class RescuePointData : ScriptableObject
    {
        [Header("Identity")]
        public string poiId;
        public string displayName;

        [TextArea(2, 5)]
        public string description;

        public string district = "Riverside";
        public RescuePointType rescuePointType;

        [Header("Rescue")]
        [Min(0)]
        public int numberOfVictims = 1;

        [Min(0)]
        public int criticalVictims;

        [Header("Scoring")]
        [Min(0)]
        public int baseScore = 100;

        [Header("Interaction")]
        [Min(1f)]
        public float discoveryRadius = 12f;

        [Min(1f)]
        public float interactionRange = 3f;

        [Min(0.1f)]
        public float interactionDuration = 3f;
    }
}
