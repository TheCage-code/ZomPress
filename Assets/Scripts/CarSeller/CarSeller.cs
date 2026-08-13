using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CarSeller : MonoBehaviour
{
    public string carId;
    public Sprite carSprite;
    public int price = 500;
    public int carDamage = 100;
    public int carHealth = 10;
    public string carTag = "Car";
    public string mainMenuSceneName = "MainMenu";
    public Button buyButton;
    public Button selectButton;

    private const string PurchasedKeyPrefix = "PurchasedCar_";
    private int normalizedCarId = -1;

    public string CarId => carId;
    public Sprite CarSprite => carSprite;
    public int Price => price;
    public int CarDamage => carDamage;
    public int CarHealth => carHealth;

    private void Start()
    {
        CacheCarId();
        InitializeCarStats();

        if (buyButton != null)
        {
            buyButton.onClick.AddListener(OnBuyButtonClicked);
        }

        if (selectButton != null)
        {
            selectButton.onClick.AddListener(OnSelectButtonClicked);
        }

        UpdateButtons();
    }

    private void CacheCarId()
    {
        if (!int.TryParse(carId, out normalizedCarId) || normalizedCarId < 1 || normalizedCarId > 9)
        {
            normalizedCarId = -1;
        }
    }

    private string GetPurchaseKey()
    {
        if (normalizedCarId < 1 || normalizedCarId > 9)
        {
            return string.Empty;
        }

        return PurchasedKeyPrefix + normalizedCarId;
    }

    private void InitializeCarStats()
    {
        // Set car damage and health based on carId (1-9)
        if (normalizedCarId > 0)
        {
            int baseDamage = 110 + (normalizedCarId * 10);  // 120, 130, 140, ..., 200
            carDamage = Mathf.RoundToInt(baseDamage * 0.9f);
            carHealth = 100 + (normalizedCarId * 10);  // 110, 120, 130, ..., 190
        }
    }

    private void OnEnable()
    {
        UpdateButtons();
    }

    public void OnBuyButtonClicked()
    {
        string purchaseKey = GetPurchaseKey();
        if (string.IsNullOrEmpty(purchaseKey))
        {
            return;
        }

        if (carSprite == null)
        {
            Debug.LogWarning("CarSeller: car sprite is not assigned.");
            return;
        }

        if (GoldManager.Instance == null)
        {
            Debug.LogWarning("CarSeller: GoldManager not found.");
            return;
        }

        if (!GoldManager.Instance.SpendGold(price))
        {
            return;
        }

        PlayerPrefs.SetInt(purchaseKey, 1);
        PlayerPrefs.Save();

        if (selectButton != null)
        {
            selectButton.gameObject.SetActive(true);
        }
        if (buyButton != null)
        {
            buyButton.interactable = false;
        }
    }

    public void OnSelectButtonClicked()
    {
        if (carSprite == null)
        {
            Debug.LogWarning("CarSeller: car sprite missing.");
            return;
        }

        if (CarSelectionManager.Instance != null)
        {
            CarSelectionManager.Instance.SetSelectedCar(carId, carSprite);
        }
        else
        {
            Debug.LogWarning("CarSeller: CarSelectionManager not found.");
        }
    }

    private void UpdateButtons()
    {
        string purchaseKey = GetPurchaseKey();
        var purchased = !string.IsNullOrEmpty(purchaseKey) && PlayerPrefs.GetInt(purchaseKey, 0) == 1;

        if (selectButton != null)
        {
            selectButton.gameObject.SetActive(purchased);
        }

        if (buyButton != null)
        {
            buyButton.interactable = !purchased;
        }
    }
}
