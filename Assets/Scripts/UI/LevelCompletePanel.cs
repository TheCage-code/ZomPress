using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelCompletePanel : MonoBehaviour
{
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private TMP_Text resultTitleText;
    [SerializeField] private TMP_Text roundGoldText;
    [SerializeField] private TMP_Text roundScoreText;
    [SerializeField] private TMP_Text highScoreText;
    [SerializeField] private TMP_Text littleKillText;
    [SerializeField] private TMP_Text normalKillText;
    [SerializeField] private TMP_Text bigKillText;
    [SerializeField] private TMP_Text bossKillText;
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string levelSceneName = "Level1";

    void Start()
    {
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(false);
        }
    }

    public void ShowLevelComplete()
    {
        ShowLevelComplete(null);
    }

    public void ShowLevelComplete(string resultTitle)
    {
        // Paneli aç
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);
            
            // Panel'in Image component'ini enable et
            Image panelImage = levelCompletePanel.GetComponent<Image>();
            if (panelImage != null)
            {
                panelImage.enabled = true;
            }
        }

        if (resultTitleText != null && !string.IsNullOrEmpty(resultTitle))
        {
            resultTitleText.text = resultTitle;
        }

        UpdateStatsDisplay();

        // Oyun durdur
        Time.timeScale = 0f;
    }

    private void UpdateStatsDisplay()
    {
        int roundGold = 0;
        int roundScore = 0;
        int highScore = 0;
        int littleKills = 0;
        int normalKills = 0;
        int bigKills = 0;
        int bossKills = 0;

        if (ScoreManager.Instance != null)
        {
            roundGold = ScoreManager.Instance.RoundGoldEarned;
            roundScore = ScoreManager.Instance.RoundScore;
            highScore = ScoreManager.Instance.HighScore;
            littleKills = ScoreManager.Instance.LittleKillCount;
            normalKills = ScoreManager.Instance.NormalKillCount;
            bigKills = ScoreManager.Instance.BigKillCount;
            bossKills = ScoreManager.Instance.BossKillCount;
        }
        else if (GoldManager.Instance != null)
        {
            roundGold = GoldManager.Instance.CurrentGold;
        }

        if (roundGoldText != null)
        {
            roundGoldText.text = roundGold.ToString();
        }

        if (roundScoreText != null)
        {
            roundScoreText.text = roundScore.ToString();
        }

        if (highScoreText != null)
        {
            highScoreText.text = highScore.ToString();
        }

        if (littleKillText != null)
        {
            littleKillText.text = littleKills.ToString();
        }

        if (normalKillText != null)
        {
            normalKillText.text = normalKills.ToString();
        }

        if (bigKillText != null)
        {
            bigKillText.text = bigKills.ToString();
        }

        if (bossKillText != null)
        {
            bossKillText.text = bossKills.ToString();
        }
    }

    public void ContinueGame()
    {
        // Oyun'u geri başlat
        Time.timeScale = 1f;

        // Paneli kapat
        if (levelCompletePanel != null)
        {
            // Image'ı disable et
            Image panelImage = levelCompletePanel.GetComponent<Image>();
            if (panelImage != null)
            {
                panelImage.enabled = false;
            }
            
            levelCompletePanel.SetActive(false);
        }
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void PlayAgain()
    {
        Time.timeScale = 1f;

        if (!string.IsNullOrEmpty(levelSceneName))
        {
            SceneManager.LoadScene(levelSceneName);
            return;
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
