using UnityEngine;
using UnityEngine.SceneManagement;

public class CarSelectionManager : MonoBehaviour
{
    public static CarSelectionManager Instance { get; private set; }
    private const string SelectedCarKey = "SelectedCarId";
    [SerializeField] private string carTag = "Car";

    private string selectedCarId;
    private Sprite selectedCarSprite;

    public string SelectedCarId => selectedCarId;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        selectedCarId = PlayerPrefs.GetString(SelectedCarKey, string.Empty);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureSelectedCarSprite();
        ApplySelectedCar();
    }

    public void SetSelectedCar(string carId, Sprite carSprite)
    {
        if (string.IsNullOrEmpty(carId) || carSprite == null)
            return;

        selectedCarId = carId;
        selectedCarSprite = carSprite;
        PlayerPrefs.SetString(SelectedCarKey, selectedCarId);
        PlayerPrefs.Save();
        ApplySelectedCar();
    }

    private void EnsureSelectedCarSprite()
    {
        if (selectedCarSprite != null)
            return;

        if (string.IsNullOrEmpty(selectedCarId))
            return;

        var sellers = FindObjectsOfType<CarSeller>();
        foreach (var seller in sellers)
        {
            if (seller.CarId == selectedCarId && seller.CarSprite != null)
            {
                selectedCarSprite = seller.CarSprite;
                return;
            }
        }
    }

    private void ApplySelectedCar()
    {
        if (selectedCarSprite == null)
            return;

        var carObj = GameObject.FindWithTag(carTag);
        if (carObj == null)
            return;

        // Apply sprite and scale
        var sr = carObj.GetComponent<SpriteRenderer>() ?? carObj.GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            sr.sprite = selectedCarSprite;
            carObj.transform.localScale = GetCarScale(selectedCarId);
        }

        var uiImage = carObj.GetComponent<UnityEngine.UI.Image>() ?? carObj.GetComponentInChildren<UnityEngine.UI.Image>();
        if (uiImage != null)
        {
            uiImage.sprite = selectedCarSprite;
        }

        // Apply car damage and health stats
        if (UpgradeManager.Instance != null && int.TryParse(selectedCarId, out int carIndex) && carIndex > 0 && carIndex <= 9)
        {
            UpgradeManager.Instance.SelectCar(carIndex - 1);
            
            var carHealthComponent = carObj.GetComponent<CarHealth>();
            if (carHealthComponent != null)
            {
                carHealthComponent.SetMaxHealth(UpgradeManager.Instance.currentCarHealth);
                carHealthComponent.SetCarDamage(UpgradeManager.Instance.currentCarDamage);
            }
        }
    }

    private Vector3 GetCarScale(string carId)
    {
        switch (carId)
        {
            case "1":
                return new Vector3(1.1f, 1.1f, 1f);
            case "2":
                return new Vector3(1.2f, 1.2f, 1f);
            case "3":
                return new Vector3(1.3f, 1.3f, 1f);
            case "4":
                return new Vector3(1.4f, 1.4f, 1f);
            case "5":
                return new Vector3(1.5f, 1.5f, 1f);
            case "6":
                return new Vector3(1.6f, 1.6f, 1f);
            case "7":
                return new Vector3(1.7f, 1.7f, 1f);
            case "8":
                return new Vector3(1.8f, 1.8f, 1f);
            case "9":
                return new Vector3(1.9f, 1.9f, 1f);
            default:
                return new Vector3(1f, 1f, 1f);
        }
    }
}
