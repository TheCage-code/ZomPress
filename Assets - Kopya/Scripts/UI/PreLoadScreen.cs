using UnityEngine;
using UnityEngine.SceneManagement;

public class PreLoadScreen : MonoBehaviour
{
    public float displayTime = 5f;
    public string mainMenuSceneName = "MainMenu";

    private float elapsedTime;

    void Start()
    {
        elapsedTime = 0f;
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;

        if (elapsedTime >= displayTime)
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}
