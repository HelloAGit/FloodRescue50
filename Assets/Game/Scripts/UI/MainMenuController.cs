using UnityEngine;
using UnityEngine.SceneManagement;

namespace FloodRescue50.UI
{
    public class MainMenuController : MonoBehaviour
    {
        private void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void StartRescue()
        {
            if (Application.CanStreamedLevelBeLoaded("FloodTown"))
                SceneManager.LoadScene("FloodTown");
            else
                Debug.LogWarning("Register FloodTown in the build scene list before starting a rescue.", this);
        }

        public void Quit()
        {
            Application.Quit();
        }
    }
}
