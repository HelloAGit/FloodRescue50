using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using FloodRescue50.Core;

namespace FloodRescue50.UI
{
    public class MissionResultsController : MonoBehaviour
    {
        [SerializeField]
        private MissionManager missionManager;

        [SerializeField]
        private GameObject resultsPanel;

        [SerializeField]
        private TMP_Text titleText;

        [SerializeField]
        private TMP_Text scoreText;

        [SerializeField]
        private TMP_Text progressText;

        [SerializeField]
        private TMP_Text timeText;

        private void Awake()
        {
            if (resultsPanel != null)
            {
                resultsPanel.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (missionManager != null)
            {
                missionManager.MissionCompleted +=
                    ShowResults;
            }
        }

        private void OnDisable()
        {
            if (missionManager != null)
            {
                missionManager.MissionCompleted -=
                    ShowResults;
            }
        }

        private void ShowResults(
            MissionResults results)
        {
            if (resultsPanel != null)
            {
                resultsPanel.SetActive(true);
            }

            if (titleText != null)
            {
                titleText.text =
                    results.AllPointsCompleted
                        ? "MISSION COMPLETE"
                        : "MISSION ENDED";
            }

            if (scoreText != null)
            {
                scoreText.text =
                    $"Score: {results.FinalScore:N0}";
            }

            if (progressText != null)
            {
                progressText.text =
                    $"Rescue Points: " +
                    $"{results.CompletedPoints}/" +
                    $"{results.TotalPoints}";
            }

            if (timeText != null)
            {
                int totalSeconds =
                    Mathf.FloorToInt(
                        results.ElapsedTime);

                int minutes =
                    totalSeconds / 60;

                int seconds =
                    totalSeconds % 60;

                timeText.text =
                    $"Mission Time: " +
                    $"{minutes:00}:{seconds:00}";
            }

            Cursor.lockState =
                CursorLockMode.None;

            Cursor.visible = true;
        }

        public void RetryMission()
        {
            SceneManager.LoadScene(
                SceneManager.GetActiveScene()
                    .buildIndex);
        }

        public void LoadMainMenu()
        {
            SceneManager.LoadScene(
                "MainMenu");
        }
    }
}
