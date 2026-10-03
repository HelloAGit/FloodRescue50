using TMPro;
using UnityEngine;
using FloodRescue50.Core;
using FloodRescue50.Player;
using FloodRescue50.Scoring;

namespace FloodRescue50.UI
{
    public class MissionHudController : MonoBehaviour
    {
        [Header("Systems")]
        [SerializeField]
        private MissionTimer missionTimer;

        [SerializeField]
        private ScoringService scoringService;

        [SerializeField]
        private MissionManager missionManager;

        [SerializeField]
        private PlayerRescueInteractor
            playerInteractor;

        [Header("UI")]
        [SerializeField]
        private TMP_Text timerText;

        [SerializeField]
        private TMP_Text scoreText;

        [SerializeField]
        private TMP_Text progressText;

        [SerializeField]
        private TMP_Text objectiveText;

        [SerializeField]
        private TMP_Text interactionText;

        private void OnEnable()
        {
            if (missionTimer != null)
            {
                missionTimer.TimerChanged +=
                    UpdateTimer;
            }

            if (scoringService != null)
            {
                scoringService.ScoreChanged +=
                    UpdateScore;
            }

            if (missionManager != null)
            {
                missionManager.ProgressChanged +=
                    UpdateProgress;
            }

            if (playerInteractor != null)
            {
                playerInteractor.PromptChanged +=
                    UpdateInteractionPrompt;
            }
        }

        private void OnDisable()
        {
            if (missionTimer != null)
            {
                missionTimer.TimerChanged -=
                    UpdateTimer;
            }

            if (scoringService != null)
            {
                scoringService.ScoreChanged -=
                    UpdateScore;
            }

            if (missionManager != null)
            {
                missionManager.ProgressChanged -=
                    UpdateProgress;
            }

            if (playerInteractor != null)
            {
                playerInteractor.PromptChanged -=
                    UpdateInteractionPrompt;
            }
        }

        private void Start()
        {
            UpdateTimer(
                missionTimer != null
                    ? missionTimer.RemainingTime
                    : 0f);

            UpdateScore(
                scoringService != null
                    ? scoringService.TotalScore
                    : 0);

            if (missionManager != null)
            {
                UpdateProgress(
                    missionManager.CompletedPoints,
                    missionManager.TotalPoints);
            }

            if (objectiveText != null)
            {
                objectiveText.text =
                    "Explore Riverside and complete rescue operations.";
            }

            UpdateInteractionPrompt(
                string.Empty,
                false);
        }

        private void UpdateTimer(
            float remainingSeconds)
        {
            if (timerText == null)
                return;

            int totalSeconds =
                Mathf.CeilToInt(
                    remainingSeconds);

            int minutes =
                totalSeconds / 60;

            int seconds =
                totalSeconds % 60;

            timerText.text =
                $"TIME {minutes:00}:{seconds:00}";
        }

        private void UpdateScore(
            int score)
        {
            if (scoreText == null)
                return;

            scoreText.text =
                $"SCORE {score:N0}";
        }

        private void UpdateProgress(
            int completed,
            int total)
        {
            if (progressText == null)
                return;

            progressText.text =
                $"RESCUE POINTS {completed}/{total}";
        }

        private void
            UpdateInteractionPrompt(
                string prompt,
                bool visible)
        {
            if (interactionText == null)
                return;

            interactionText.gameObject
                .SetActive(visible);

            interactionText.text = prompt;
        }
    }
}
