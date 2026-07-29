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
    public int speedUpgradeCostBase = 10;
    public int timeUpgradeCostBase = 10;

    [Header("Upgrade Bonuses")]
    public float speedBonusPerPurchase = 1f;
    public float timeBonusPerPurchase = 5f;

    [Header("Car Damage")]
    public int[] carDamageValues = new int[] { 99, 108, 117, 126, 135, 144, 153, 162, 171 };
    
    [Header("Car Health")]
    public int[] carHealthValues = new int[] { 110, 120, 130, 140, 150, 160, 170, 180, 190 };
    
    private int selectedCarIndex = 0;

    public int speedUpgradeCount { get; private set; }
    public int timeUpgradeCount { get; private set; }

    public float totalSpeedBonus => speedUpgradeCount * speedBonusPerPurchase;
    public float totalTimeBonus => timeUpgradeCount * timeBonusPerPurchase;
    public int currentCarDamage => carDamageValues[selectedCarIndex];
    public int currentCarHealth => carHealthValues[selectedCarIndex];
    
    // Dinamik upgrade cost'ları
    public int speedUpgradeCost => speedUpgradeCostBase * (speedUpgradeCount + 1);
    public int timeUpgradeCost => timeUpgradeCostBase * (timeUpgradeCount + 1);

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
        PlayerPrefs.SetInt("SelectedCarIndex", selectedCarIndex);
        PlayerPrefs.Save();
    }

    void LoadUpgrades()
    {
        speedUpgradeCount = PlayerPrefs.GetInt("SpeedUpgradeCount", 0);
        timeUpgradeCount = PlayerPrefs.GetInt("TimeUpgradeCount", 0);
        selectedCarIndex = PlayerPrefs.GetInt("SelectedCarIndex", 0);
    }

    public void SelectCar(int carIndex)
    {
        if (carIndex >= 0 && carIndex < carDamageValues.Length)
        {
            selectedCarIndex = carIndex;
            SaveUpgrades();
        }
    }
}
