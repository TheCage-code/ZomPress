using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    private static UpgradeManager _instance;
    public static UpgradeManager Instance
    {
        get
        {
            return _instance;
        }
    }

    [Header("Upgrade Costs")]
    public int speedUpgradeCost = 10;
    public int timeUpgradeCost = 10;

    [Header("Upgrade Bonuses")]
    public float speedBonusPerPurchase = 1f;
    public float timeBonusPerPurchase = 5f;

    public int speedUpgradeCount { get; private set; }
    public int timeUpgradeCount { get; private set; }

    public float totalSpeedBonus => speedUpgradeCount * speedBonusPerPurchase;
    public float totalTimeBonus => timeUpgradeCount * timeBonusPerPurchase;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        LoadUpgrades();
    }

    void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    public bool CanBuySpeedUpgrade()
    {
        return GoldManager.Instance != null && GoldManager.Instance.CurrentGold >= speedUpgradeCost;
    }

    public bool CanBuyTimeUpgrade()
    {
        return GoldManager.Instance != null && GoldManager.Instance.CurrentGold >= timeUpgradeCost;
    }

    public bool BuySpeedUpgrade()
    {
        if (!CanBuySpeedUpgrade())
            return false;

        if (GoldManager.Instance.SpendGold(speedUpgradeCost))
        {
            speedUpgradeCount++;
            SaveUpgrades();
            return true;
        }

        return false;
    }

    public bool BuyTimeUpgrade()
    {
        if (!CanBuyTimeUpgrade())
            return false;

        if (GoldManager.Instance.SpendGold(timeUpgradeCost))
        {
            timeUpgradeCount++;
            SaveUpgrades();
            return true;
        }

        return false;
    }

    void SaveUpgrades()
    {
        PlayerPrefs.SetInt("SpeedUpgradeCount", speedUpgradeCount);
        PlayerPrefs.SetInt("TimeUpgradeCount", timeUpgradeCount);
        PlayerPrefs.Save();
    }

    void LoadUpgrades()
    {
        speedUpgradeCount = PlayerPrefs.GetInt("SpeedUpgradeCount", 0);
        timeUpgradeCount = PlayerPrefs.GetInt("TimeUpgradeCount", 0);
    }
}
