using System;
using UnityEngine;

public class GoldManager : MonoBehaviour
{
    private static GoldManager _instance;
    public static GoldManager Instance
    {
        get
        {
            return _instance;
        }
    }

    [Header("Gold Settings")]
    public int startingGold = 0;

    public int CurrentGold { get; private set; }
    public event Action<int> OnGoldChanged;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        LoadGold();
    }

    void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    void Start()
    {
        OnGoldChanged?.Invoke(CurrentGold);
    }

    void OnApplicationQuit()
    {
        SaveGold();
    }

    public void AddGold(int amount)
    {
        if (amount <= 0)
            return;

        CurrentGold += amount;
        SaveGold();
        OnGoldChanged?.Invoke(CurrentGold);
    }

    public bool SpendGold(int amount)
    {
        if (amount <= 0 || CurrentGold < amount)
            return false;

        CurrentGold -= amount;
        SaveGold();
        OnGoldChanged?.Invoke(CurrentGold);
        return true;
    }

    void SaveGold()
    {
        PlayerPrefs.SetInt("PlayerGold", CurrentGold);
        PlayerPrefs.Save();
    }

    void LoadGold()
    {
        if (PlayerPrefs.HasKey("PlayerGold"))
        {
            CurrentGold = PlayerPrefs.GetInt("PlayerGold");
        }
        else
        {
            CurrentGold = startingGold;
        }
    }
}
