using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PreLoadScreen : MonoBehaviour
{
    public float displayTime = 5f;
    public string mainMenuSceneName = "MainMenu";
    public TMP_Text titleText;
    public float letterDelay = 0.2f;
    public GameObject imageToActivate;
    public float imageActivationDelay = 0.31f;

    private float elapsedTime;

    void Start()
    {
        elapsedTime = 0f;
        StartCoroutine(TypeTitle());
        StartCoroutine(ActivateImage());
    }

    IEnumerator ActivateImage()
    {
        if (imageToActivate == null)
            yield break;

        yield return new WaitForSecondsRealtime(imageActivationDelay);
        imageToActivate.SetActive(true);
    }

    IEnumerator TypeTitle()
    {
        if (titleText == null)
            yield break;

        string title = "ZOMPRESS";
        titleText.text = "";

        foreach (char letter in title)
        {
            titleText.text += letter;
            yield return new WaitForSeconds(letterDelay);
        }
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
