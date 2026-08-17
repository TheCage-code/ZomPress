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
    public TMP_Text damageCostText;
    public TMP_Text turretCostText;
    public TMP_Text speedBonusText;
    public TMP_Text timeBonusText;
    public TMP_Text damageBonusText;
    public TMP_Text turretStateText;
    public Button buySpeedButton;
    public Button buyTimeButton;
    public Button buyDamageButton;
    public Button buyTurretButton;
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
        if (buyDamageButton != null)
        {
            buyDamageButton.onClick.AddListener(BuyDamageUpgrade);
        }
        if (buyTurretButton != null)
        {
            buyTurretButton.onClick.AddListener(BuyTurret);
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

    void BuyDamageUpgrade()
    {
        if (UpgradeManager.Instance != null && UpgradeManager.Instance.BuyDamageUpgrade())
        {
            UpdateUI();
        }
    }

    void BuyTurret()
    {
        if (UpgradeManager.Instance != null && UpgradeManager.Instance.BuyTurret())
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
            speedCostText.text = UpgradeManager.Instance.IsSpeedUpgradeMaxed ? "max" : UpgradeManager.Instance.speedUpgradeCost.ToString();
        }

        if (timeCostText != null && UpgradeManager.Instance != null)
        {
            timeCostText.text = UpgradeManager.Instance.IsTimeUpgradeMaxed ? "max" : UpgradeManager.Instance.timeUpgradeCost.ToString();
        }

        if (damageCostText != null && UpgradeManager.Instance != null)
        {
            damageCostText.text = UpgradeManager.Instance.IsDamageUpgradeMaxed ? "max" : UpgradeManager.Instance.damageUpgradeCost.ToString();
        }

        if (turretCostText != null && UpgradeManager.Instance != null)
        {
            turretCostText.text = UpgradeManager.Instance.hasTurret ? "owned" : UpgradeManager.Instance.turretCost.ToString();
        }

        if (speedBonusText != null && UpgradeManager.Instance != null)
        {
            speedBonusText.text = "+" + UpgradeManager.Instance.totalSpeedBonus.ToString();
        }

        if (timeBonusText != null && UpgradeManager.Instance != null)
        {
            timeBonusText.text = "+" + UpgradeManager.Instance.totalTimeBonus.ToString() + "s";
        }

        if (damageBonusText != null && UpgradeManager.Instance != null)
        {
            damageBonusText.text = "+" + UpgradeManager.Instance.totalDamageBonus.ToString();
        }

        if (turretStateText != null && UpgradeManager.Instance != null)
        {
            turretStateText.text = UpgradeManager.Instance.hasTurret ? "Owned" : "Not Owned";
        }

        if (buyTurretButton != null && UpgradeManager.Instance != null)
        {
            buyTurretButton.interactable = !UpgradeManager.Instance.hasTurret;
        }
    }
}
