using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelCompletePanel : MonoBehaviour
{
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private TMP_Text goldEarnedText;
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    
    private int goldEarned;

    void Start()
    {
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(false);
        }
    }

    public void ShowLevelComplete()
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

        // O runda kazanılan altını göster
        if (GoldManager.Instance != null)
        {
            goldEarned = GoldManager.Instance.CurrentGold;
            UpdateGoldDisplay();
        }

        // Oyun durdur
        Time.timeScale = 0f;
    }

    private void UpdateGoldDisplay()
    {
        if (goldEarnedText != null)
        {
            goldEarnedText.text = goldEarned.ToString();
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
}
