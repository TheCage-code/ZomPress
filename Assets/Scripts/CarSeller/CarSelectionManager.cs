using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CarSelectionManager : MonoBehaviour
{
    public static CarSelectionManager Instance { get; private set; }
    private const string SelectedCarKey = "SelectedCarId";
    [SerializeField] private string carTag = "Car";
    [SerializeField] private Sprite[] carSprites = new Sprite[9];

    private string selectedCarId;
    private Sprite selectedCarSprite;

    public string SelectedCarId => selectedCarId;
    public Sprite SelectedCarSprite => selectedCarSprite;
    public event Action<Sprite, string> SelectedCarChanged;

    // Guarantees an instance exists from the very first scene, so the saved car
    // is applied even if the player never visits CarShop after launching the game.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null)
            return;

        var prefab = Resources.Load<CarSelectionManager>("CarSelectionManager");
        if (prefab != null)
        {
            Instantiate(prefab);
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            DestroyImmediate(gameObject);
            return;
        }

        Instance = this;
        
        // Make sure this is a root GameObject before calling DontDestroyOnLoad
        if (transform.parent != null)
        {
            transform.SetParent(null, false);
        }
        
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

    private void Start()
    {
        StartCoroutine(ApplySelectedCarNextFrame());
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureSelectedCarSprite();
        ApplySelectedCar();
        NotifySelectedCarChanged();
        StartCoroutine(ApplySelectedCarNextFrame());
    }

    private System.Collections.IEnumerator ApplySelectedCarNextFrame()
    {
        for (int attempt = 0; attempt < 30; attempt++)
        {
            yield return null;
            EnsureSelectedCarSprite();
            ApplySelectedCar();
            NotifySelectedCarChanged();

            if (GameObject.FindWithTag(carTag) != null && selectedCarSprite != null)
                yield break;
        }
    }

    public void SetSelectedCar(string carId, Sprite carSprite)
    {
        if (string.IsNullOrEmpty(carId) || carSprite == null)
            return;

        selectedCarId = carId;
        selectedCarSprite = carSprite;
        PlayerPrefs.SetString(SelectedCarKey, selectedCarId);
        if (int.TryParse(selectedCarId, out int carIndex) && carIndex >= 1 && carIndex <= 9)
        {
            PlayerPrefs.SetInt("SelectedCarIndex", carIndex - 1);
        }
        PlayerPrefs.Save();
        ApplySelectedCar();
        NotifySelectedCarChanged();
    }

    public bool TryGetSelectedCar(out string carId, out Sprite carSprite)
    {
        EnsureSelectedCarSprite();
        carId = selectedCarId;
        carSprite = selectedCarSprite;
        return !string.IsNullOrEmpty(carId) && carSprite != null;
    }

    private void EnsureSelectedCarSprite()
    {
        if (selectedCarSprite != null)
            return;

        if (string.IsNullOrEmpty(selectedCarId))
            return;

        if (int.TryParse(selectedCarId, out int carIndex) && carIndex >= 1 && carIndex <= carSprites.Length)
        {
            selectedCarSprite = carSprites[carIndex - 1];
            if (selectedCarSprite != null)
                return;
        }

        var sellers = FindObjectsByType<CarSeller>(FindObjectsInactive.Include, FindObjectsSortMode.None);
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
        var carObj = GameObject.FindWithTag(carTag);
        if (carObj == null)
            return;

        // Apply sprite and scale when sprite data is available.
        if (selectedCarSprite != null)
        {
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
        }

        // Apply car damage and health stats
        if (UpgradeManager.Instance != null && int.TryParse(selectedCarId, out int carIndex) && carIndex > 0 && carIndex <= 9)
        {
            UpgradeManager.Instance.SelectCar(carIndex - 1);
            
            var carHealthComponent = carObj.GetComponent<CarHealth>();
            if (carHealthComponent != null)
            {
                carHealthComponent.SetCarLevel(carIndex);
                carHealthComponent.SetMaxHealth(UpgradeManager.Instance.currentCarHealth);
                carHealthComponent.SetCarDamage(UpgradeManager.Instance.currentCarDamage);
            }

            if (UpgradeManager.Instance != null)
            {
                UpgradeManager.Instance.EnsureTurretOnCar();
                UpgradeManager.Instance.SetTurretActiveState(UpgradeManager.Instance.hasTurret);
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

    private void NotifySelectedCarChanged()
    {
        if (selectedCarSprite == null)
            return;

        SelectedCarChanged?.Invoke(selectedCarSprite, selectedCarId);
    }
}
