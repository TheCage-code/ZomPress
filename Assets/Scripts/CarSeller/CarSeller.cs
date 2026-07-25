using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CarSeller : MonoBehaviour
{
    public string carId;
    public Sprite carSprite;
    public int price = 500;
    public string carTag = "Car";
    public string mainMenuSceneName = "MainMenu";
    public Button buyButton;
    public Button selectButton;

    private const string PurchasedKeyPrefix = "PurchasedCar_";

    public string CarId => carId;
    public Sprite CarSprite => carSprite;
    public int Price => price;

    private void Start()
    {
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

    private void OnEnable()
    {
        UpdateButtons();
    }

    public void OnBuyButtonClicked()
    {
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

        PlayerPrefs.SetInt(PurchasedKeyPrefix + carId, 1);
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
        var purchased = PlayerPrefs.GetInt(PurchasedKeyPrefix + carId, 0) == 1;

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
