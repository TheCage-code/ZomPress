using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Scene Names")]
    public string levelSceneName = "Level1";
    public string optionsSceneName = "Options";
    public string shopSceneName = "Shop";
    public string carShopSceneName = "CarShop";

    public void StartGame()
    {
        if (!string.IsNullOrEmpty(levelSceneName))
        {
            SceneManager.LoadScene(levelSceneName);
        }
    }

    public void OpenOptions()
    {
        // Eğer options için ayrı bir sahne kullanmak istiyorsan burada aç.
        if (!string.IsNullOrEmpty(optionsSceneName))
        {
            SceneManager.LoadScene(optionsSceneName);
        }
    }

    public void OpenShop()
    {
        if (!string.IsNullOrEmpty(shopSceneName))
        {
            SceneManager.LoadScene(shopSceneName);
        }
    }

    public void OpenCarShop()
    {
        if (!string.IsNullOrEmpty(carShopSceneName))
        {
            SceneManager.LoadScene(carShopSceneName);
        }
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
