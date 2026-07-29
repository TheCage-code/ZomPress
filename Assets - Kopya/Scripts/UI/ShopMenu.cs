using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class ShopMenu : MonoBehaviour
{
    public string levelSceneName = "Level1";
    public string mainMenuSceneName = "MainMenu";
    public TMP_Text goldText;
    public TMP_Text speedCostText;
    public TMP_Text timeCostText;
    public TMP_Text speedBonusText;
    public TMP_Text timeBonusText;
    public Button buySpeedButton;
    public Button buyTimeButton;
    public Button startLevelButton;
    public Button exitButton;

    void Start()
    {
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(OpenMainMenu);
        }
        if (buySpeedButton != null)
        {
            buySpeedButton.onClick.AddListener(BuySpeedUpgrade);
        }
        if (buyTimeButton != null)
        {
            buyTimeButton.onClick.AddListener(BuyTimeUpgrade);
        }
        if (startLevelButton != null)
        {
            startLevelButton.onClick.AddListener(StartLevel);
        }

        UpdateUI();
    }

    void OnEnable()
    {
        UpdateUI();
    }

    void Update()
    {
        UpdateUI();
    }

    void BuySpeedUpgrade()
    {
        if (UpgradeManager.Instance != null && UpgradeManager.Instance.BuySpeedUpgrade())
        {
            UpdateUI();
        }
    }

    void BuyTimeUpgrade()
    {
        if (UpgradeManager.Instance != null && UpgradeManager.Instance.BuyTimeUpgrade())
        {
            UpdateUI();
        }
    }

    void OpenMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    void StartLevel()
    {
        SceneManager.LoadScene(levelSceneName);
    }

    void UpdateUI()
    {
        if (goldText != null && GoldManager.Instance != null)
        {
            goldText.text = GoldManager.Instance.CurrentGold.ToString();
        }

        if (speedCostText != null && UpgradeManager.Instance != null)
        {
            speedCostText.text = UpgradeManager.Instance.speedUpgradeCost.ToString();
        }

        if (timeCostText != null && UpgradeManager.Instance != null)
        {
            timeCostText.text = UpgradeManager.Instance.timeUpgradeCost.ToString();
        }

        if (speedBonusText != null && UpgradeManager.Instance != null)
        {
            speedBonusText.text = "+" + UpgradeManager.Instance.totalSpeedBonus.ToString();
        }

        if (timeBonusText != null && UpgradeManager.Instance != null)
        {
            timeBonusText.text = "+" + UpgradeManager.Instance.totalTimeBonus.ToString() + "s";
        }
    }
}
