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
    
    private static readonly int[] speedUpgradeCosts = { 10, 100, 500, 750, 1000 };
    private static readonly int[] damageUpgradeCosts = { 10, 100, 500, 1000, 2000 };
    private static readonly int[] timeUpgradeCosts = {
        10, 50, 100, 150, 200, 250, 300, 350, 400, 450,
        500, 550, 600, 650, 700, 750, 800, 850, 900, 950,
        1000, 1050, 1100, 1150, 1200, 1250, 1300, 1350, 1400, 1450
    };


    [Header("Upgrade Bonuses")]
    public float speedBonusPerPurchase = 1f;
    public float timeBonusPerPurchase = 5f;
    public int damageBonusPerPurchase = 10;
    

    [Header("Max Upgrades")]
    private const int MAX_SPEED_UPGRADES = 5;
    private const int MAX_DAMAGE_UPGRADES = 5;
    private const int MAX_TIME_UPGRADES = 30;

    [Header("Car Damage")]
    public int[] carDamageValues = new int[] { 99, 108, 117, 126, 135, 144, 153, 162, 171 };
    
    [Header("Car Health")]
    public int[] carHealthValues = new int[] { 110, 120, 130, 140, 150, 160, 170, 180, 190 };
    
    private int _selectedCarIndex = 0;

    public int speedUpgradeCount { get; private set; }
    public int damageUpgradeCount { get; private set; }
    public int timeUpgradeCount { get; private set; }
    public int selectedCarIndex { get; private set; }

    public float totalSpeedBonus => speedUpgradeCount * speedBonusPerPurchase;
    public float totalTimeBonus => timeUpgradeCount * timeBonusPerPurchase;
    public int totalDamageBonus => damageUpgradeCount * 10;
    public int currentCarDamage => carDamageValues[_selectedCarIndex] + totalDamageBonus;
    public int currentCarHealth => carHealthValues[_selectedCarIndex];
    
    // Max kontrol
    public bool IsSpeedUpgradeMaxed => speedUpgradeCount >= MAX_SPEED_UPGRADES;
    public bool IsDamageUpgradeMaxed => damageUpgradeCount >= MAX_DAMAGE_UPGRADES;
    public bool IsTimeUpgradeMaxed => timeUpgradeCount >= MAX_TIME_UPGRADES;
    
    // Hiz upgrade maliyeti sirali olarak artar: 10, 100, 500, 750, 1000
    public int speedUpgradeCost => IsSpeedUpgradeMaxed ? speedUpgradeCosts[MAX_SPEED_UPGRADES - 1] : speedUpgradeCosts[speedUpgradeCount];
    public int damageUpgradeCost => IsDamageUpgradeMaxed ? damageUpgradeCosts[MAX_DAMAGE_UPGRADES - 1] : damageUpgradeCosts[damageUpgradeCount];
    public int timeUpgradeCost => IsTimeUpgradeMaxed ? timeUpgradeCosts[MAX_TIME_UPGRADES - 1] : timeUpgradeCosts[timeUpgradeCount];

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            DestroyImmediate(gameObject);
            return;
        }

        _instance = this;
        
        // Make sure this is a root GameObject before calling DontDestroyOnLoad
        if (transform.parent != null)
        {
            transform.SetParent(null, false);
        }
        
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
        return !IsSpeedUpgradeMaxed && GoldManager.Instance != null && GoldManager.Instance.CurrentGold >= speedUpgradeCost;
    }

    public bool CanBuyDamageUpgrade()
    {
        return !IsDamageUpgradeMaxed && GoldManager.Instance != null && GoldManager.Instance.CurrentGold >= damageUpgradeCost;
    }

    public bool CanBuyTimeUpgrade()
    {
        return !IsTimeUpgradeMaxed && GoldManager.Instance != null && GoldManager.Instance.CurrentGold >= timeUpgradeCost;
    }

    public bool BuySpeedUpgrade()
    {
        if (!CanBuySpeedUpgrade() || IsSpeedUpgradeMaxed)
            return false;

        if (GoldManager.Instance.SpendGold(speedUpgradeCost))
        {
            speedUpgradeCount++;
            SaveUpgrades();
            return true;
        }

        return false;
    }

    public bool BuyDamageUpgrade()
    {
        if (!CanBuyDamageUpgrade() || IsDamageUpgradeMaxed)
            return false;

        if (GoldManager.Instance.SpendGold(damageUpgradeCost))
        {
            damageUpgradeCount++;
            SaveUpgrades();
            return true;
        }

        return false;
    }

    public bool BuyTimeUpgrade()
    {
        if (!CanBuyTimeUpgrade() || IsTimeUpgradeMaxed)
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
        PlayerPrefs.SetInt("DamageUpgradeCount", damageUpgradeCount);
        PlayerPrefs.SetInt("TimeUpgradeCount", timeUpgradeCount);
        PlayerPrefs.SetInt("SelectedCarIndex", _selectedCarIndex);
        PlayerPrefs.Save();
    }

    void LoadUpgrades()
    {
        speedUpgradeCount = PlayerPrefs.GetInt("SpeedUpgradeCount", 0);
        damageUpgradeCount = PlayerPrefs.GetInt("DamageUpgradeCount", 0);
        timeUpgradeCount = PlayerPrefs.GetInt("TimeUpgradeCount", 0);
        _selectedCarIndex = PlayerPrefs.GetInt("SelectedCarIndex", 0);

        speedUpgradeCount = Mathf.Clamp(speedUpgradeCount, 0, MAX_SPEED_UPGRADES);
        damageUpgradeCount = Mathf.Clamp(damageUpgradeCount, 0, MAX_DAMAGE_UPGRADES);
        timeUpgradeCount = Mathf.Clamp(timeUpgradeCount, 0, MAX_TIME_UPGRADES);
        _selectedCarIndex = Mathf.Clamp(_selectedCarIndex, 0, carDamageValues.Length - 1);
    }

    public void SelectCar(int carIndex)
    {
        if (carIndex >= 0 && carIndex < carDamageValues.Length)
        {
            _selectedCarIndex = carIndex;
            SaveUpgrades();
        }
    }
}
