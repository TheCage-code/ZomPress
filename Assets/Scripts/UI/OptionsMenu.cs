using UnityEngine;
using UnityEngine.SceneManagement;

public class OptionsMenu : MonoBehaviour
{
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    public void BackToMainMenu()
    {
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}
