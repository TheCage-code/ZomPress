using UnityEngine;
using UnityEngine.SceneManagement;

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
    public int damageUpgradeCostBase = 100;
    public int turretCost = 5000;
    private static readonly int[] speedUpgradeCosts = { 10, 100, 500, 750, 1000 };
    private static readonly int[] damageUpgradeCosts = { 100, 250, 500, 750, 1000 };

    [Header("Upgrade Bonuses")]
    public float speedBonusPerPurchase = 1f;
    public float timeBonusPerPurchase = 5f;
    public int damageBonusPerPurchase = 5;

    [Header("Max Upgrades")]
    private const int MAX_SPEED_UPGRADES = 5;
    private const int MAX_TIME_UPGRADES = 20;
    private const int MAX_DAMAGE_UPGRADES = 5;

    [Header("Car Damage")]
    public int[] carDamageValues = new int[] { 99, 108, 117, 126, 135, 144, 153, 162, 171 };
    
    [Header("Car Health")]
    public int[] carHealthValues = new int[] { 110, 120, 130, 140, 150, 160, 170, 180, 190 };
    
    private int selectedCarIndex = 0;

    public int speedUpgradeCount { get; private set; }
    public int timeUpgradeCount { get; private set; }
    public int damageUpgradeCount { get; private set; }
    public bool hasTurret { get; private set; }

    public float totalSpeedBonus => speedUpgradeCount * speedBonusPerPurchase;
    public float totalTimeBonus => timeUpgradeCount * timeBonusPerPurchase;
    public int totalDamageBonus => damageUpgradeCount * damageBonusPerPurchase;
    public int currentCarDamage => carDamageValues[selectedCarIndex] + totalDamageBonus;
    public int currentCarHealth => carHealthValues[selectedCarIndex];
    
    // Max kontrol
    public bool IsSpeedUpgradeMaxed => speedUpgradeCount >= MAX_SPEED_UPGRADES;
    public bool IsTimeUpgradeMaxed => timeUpgradeCount >= MAX_TIME_UPGRADES;
    public bool IsDamageUpgradeMaxed => damageUpgradeCount >= MAX_DAMAGE_UPGRADES;
    
    // Hiz upgrade maliyeti sirali olarak artar: 10, 100, 500, 750, 1000
    public int speedUpgradeCost => IsSpeedUpgradeMaxed ? speedUpgradeCosts[MAX_SPEED_UPGRADES - 1] : speedUpgradeCosts[speedUpgradeCount];
    public int timeUpgradeCost => timeUpgradeCostBase * (timeUpgradeCount + 1);
    public int damageUpgradeCost => IsDamageUpgradeMaxed ? damageUpgradeCosts[MAX_DAMAGE_UPGRADES - 1] : damageUpgradeCosts[damageUpgradeCount];

    void Awake()
    {
        var existingManagers = FindObjectsOfType<UpgradeManager>();
        if (existingManagers.Length > 1)
        {
            for (int i = 1; i < existingManagers.Length; i++)
            {
                if (existingManagers[i] != null)
                {
                    Destroy(existingManagers[i].gameObject);
                }
            }
        }

        if (_instance != null && _instance != this)
        {
            DestroyImmediate(gameObject);
            return;
        }

        _instance = this;

        if (transform.parent != null)
        {
            transform.SetParent(null, false);
        }

        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        LoadUpgrades();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        LoadUpgrades();
        ApplyTurretState();
    }

    void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public bool CanBuySpeedUpgrade()
    {
        return !IsSpeedUpgradeMaxed && GoldManager.Instance != null && GoldManager.Instance.CurrentGold >= speedUpgradeCost;
    }

    public bool CanBuyTimeUpgrade()
    {
        return !IsTimeUpgradeMaxed && GoldManager.Instance != null && GoldManager.Instance.CurrentGold >= timeUpgradeCost;
    }

    public bool CanBuyDamageUpgrade()
    {
        return !IsDamageUpgradeMaxed && GoldManager.Instance != null && GoldManager.Instance.CurrentGold >= damageUpgradeCost;
    }

    public bool CanBuyTurret()
    {
        return !hasTurret && GoldManager.Instance != null && GoldManager.Instance.CurrentGold >= turretCost;
    }

    public bool BuyTurret()
    {
        if (!CanBuyTurret())
            return false;

        if (GoldManager.Instance.SpendGold(turretCost))
        {
            hasTurret = true;
            SaveUpgrades();
            ApplyTurretState();
            return true;
        }

        return false;
    }

    public void SetTurretActiveState(bool active)
    {
        GameObject carObject = GameObject.FindGameObjectWithTag("Car");
        if (carObject == null)
            return;

        Transform turretPivot = carObject.transform.Find("TurretPivot");
        if (turretPivot == null)
            return;

        turretPivot.gameObject.SetActive(active);

        var turretController = turretPivot.GetComponent<TurretController>();
        if (turretController != null)
        {
            turretController.SetTurretVisible(active);
        }
    }

    public void ApplyTurretState()
    {
        if (hasTurret)
        {
            EnsureTurretOnCar();
            SetTurretActiveState(true);
            return;
        }

        SetTurretActiveState(false);
    }

    public void EnsureTurretOnCar()
    {
        GameObject carObject = GameObject.FindGameObjectWithTag("Car");
        if (carObject == null)
            return;

        Transform turretPivot = carObject.transform.Find("TurretPivot");

        if (!hasTurret)
        {
            if (turretPivot != null)
            {
                Destroy(turretPivot.gameObject);
            }
            return;
        }

        if (turretPivot != null)
        {
            turretPivot.gameObject.SetActive(true);

            var turretController = turretPivot.GetComponent<TurretController>();
            if (turretController != null)
            {
                turretController.SetTurretVisible(true);
            }

            return;
        }

        GameObject newTurretPivot = new GameObject("TurretPivot");
        newTurretPivot.transform.SetParent(carObject.transform, false);
        newTurretPivot.transform.localPosition = new Vector3(0f, 0.7f, 0f);

        var turretControllerNew = newTurretPivot.AddComponent<TurretController>();
        turretControllerNew.turretPivot = newTurretPivot.transform;

        GameObject turretSpriteObject = new GameObject("TurretSprite");
        turretSpriteObject.transform.SetParent(newTurretPivot.transform, false);
        turretSpriteObject.transform.localPosition = Vector3.zero;

        SpriteRenderer sr = turretSpriteObject.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 5;
        turretControllerNew.turretSprite = sr;
        turretControllerNew.SetTurretVisible(true);
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

    void SaveUpgrades()
    {
        PlayerPrefs.SetInt("SpeedUpgradeCount", speedUpgradeCount);
        PlayerPrefs.SetInt("TimeUpgradeCount", timeUpgradeCount);
        PlayerPrefs.SetInt("DamageUpgradeCount", damageUpgradeCount);
        PlayerPrefs.SetInt("SelectedCarIndex", selectedCarIndex);
        PlayerPrefs.SetInt("HasTurret", hasTurret ? 1 : 0);
        PlayerPrefs.Save();
    }

    void LoadUpgrades()
    {
        speedUpgradeCount = PlayerPrefs.GetInt("SpeedUpgradeCount", 0);
        timeUpgradeCount = PlayerPrefs.GetInt("TimeUpgradeCount", 0);
        damageUpgradeCount = PlayerPrefs.GetInt("DamageUpgradeCount", 0);
        selectedCarIndex = PlayerPrefs.GetInt("SelectedCarIndex", 0);
        hasTurret = PlayerPrefs.GetInt("HasTurret", 0) == 1;

        speedUpgradeCount = Mathf.Clamp(speedUpgradeCount, 0, MAX_SPEED_UPGRADES);
        timeUpgradeCount = Mathf.Clamp(timeUpgradeCount, 0, MAX_TIME_UPGRADES);
        damageUpgradeCount = Mathf.Clamp(damageUpgradeCount, 0, MAX_DAMAGE_UPGRADES);

        if (hasTurret)
        {
            EnsureTurretOnCar();
        }

        ApplyTurretState();
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
