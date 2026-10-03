using UnityEngine;
using UnityEngine.SceneManagement;

namespace FloodRescue50.Core
{
    public class BootController : MonoBehaviour
    {
        private void Start()
        {
            if (Application.CanStreamedLevelBeLoaded("MainMenu"))
                SceneManager.LoadScene("MainMenu");
            else
                Debug.LogWarning("Register MainMenu in the build scene list before using Boot.", this);
        }
    }
}
