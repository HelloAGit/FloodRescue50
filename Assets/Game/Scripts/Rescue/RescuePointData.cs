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

        private void OnValidate()
        {
            ValidateConfiguration();
        }

        public void ValidateConfiguration()
        {
            if (string.IsNullOrWhiteSpace(poiId))
                Debug.LogWarning("RescuePointData has an empty POI ID; mission uses point identity as fallback.", this);

            bool invalid = numberOfVictims < 0 || criticalVictims < 0 ||
                criticalVictims > numberOfVictims || baseScore < 0 ||
                !ValidPositive(discoveryRadius) || !ValidPositive(interactionRange) ||
                !ValidPositive(interactionDuration) || discoveryRadius < interactionRange ||
                discoveryRadius < 1f || interactionRange < 1f || interactionDuration < 0.1f;
            if (invalid)
                Debug.LogWarning("Invalid rescue configuration; applying safe victim, score and interaction limits.", this);

            numberOfVictims = Mathf.Max(0, numberOfVictims);
            criticalVictims = Mathf.Clamp(criticalVictims, 0, numberOfVictims);
            baseScore = Mathf.Max(0, baseScore);
            interactionRange = ValidPositive(interactionRange) ? Mathf.Max(1f, interactionRange) : 3f;
            discoveryRadius = ValidPositive(discoveryRadius) ? Mathf.Max(1f, discoveryRadius) : 12f;
            discoveryRadius = Mathf.Max(discoveryRadius, interactionRange);
            interactionDuration = ValidPositive(interactionDuration) ? Mathf.Max(0.1f, interactionDuration) : 3f;
        }

        private static bool ValidPositive(float value)
        {
            return value > 0f && !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
