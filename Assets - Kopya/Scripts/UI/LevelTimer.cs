using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelTimer : MonoBehaviour
{
    public float levelTime = 10f;
    public string mainMenuSceneName = "MainMenu";
    public TMP_Text timerText;
    public bool stopOnTimeUp = true;

    private float remainingTime;
    private bool isFinished;

    void Awake()
    {
        if (timerText == null)
        {
            timerText = GetComponent<TMP_Text>();
        }
    }

    void Start()
    {
        float totalTime = levelTime;

        if (UpgradeManager.Instance != null)
        {
            totalTime += UpgradeManager.Instance.totalTimeBonus;
        }

        remainingTime = totalTime;
        isFinished = false;
        UpdateTimerUI();
    }

    void Update()
    {
        if (isFinished)
            return;

        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            UpdateTimerUI();
            EndLevel();
            return;
        }

        UpdateTimerUI();
    }

    public void AddTime(float seconds)
    {
        remainingTime += seconds;
        if (remainingTime < 0f)
            remainingTime = 0f;

        UpdateTimerUI();
    }

    public void ResetTimer()
    {
        float totalTime = levelTime;

        if (UpgradeManager.Instance != null)
        {
            totalTime += UpgradeManager.Instance.totalTimeBonus;
        }

        remainingTime = totalTime;
        isFinished = false;
        Time.timeScale = 1f;
        UpdateTimerUI();
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            timerText.text = Mathf.CeilToInt(remainingTime).ToString();
        }
    }

    void EndLevel()
    {
        if (isFinished)
            return;

        isFinished = true;

        if (stopOnTimeUp)
        {
            Time.timeScale = 0f;
        }

        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}
