using UnityEngine;
using UnityEngine.SceneManagement;

public class CarShopNavigation : MonoBehaviour
{
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    public void ReturnToMainMenu()
    {
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            Debug.LogWarning("CarShopNavigation: mainMenuSceneName is not set.");
        }
    }
}
