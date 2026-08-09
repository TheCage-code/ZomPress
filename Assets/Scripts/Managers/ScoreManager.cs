using UnityEngine;
using UnityEngine.SceneManagement;

public class ScoreManager : MonoBehaviour
{
    private const string HighScoreKey = "HighScore";

    private static ScoreManager _instance;
    public static ScoreManager Instance => _instance;

    [Header("Zombie Points")]
    [SerializeField] private int littleZombiePoints = 5;
    [SerializeField] private int normalZombiePoints = 10;
    [SerializeField] private int bigZombiePoints = 25;
    [SerializeField] private int bossZombiePoints = 100;

    public int RoundScore { get; private set; }
    public int HighScore { get; private set; }
    public int RoundGoldEarned { get; private set; }

    public int LittleKillCount { get; private set; }
    public int NormalKillCount { get; private set; }
    public int BigKillCount { get; private set; }
    public int BossKillCount { get; private set; }

    private int lastObservedGold = int.MinValue;
    private bool isGoldSubscribed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoCreate()
    {
        if (_instance != null)
            return;

        GameObject managerObject = new GameObject("ScoreManager");
        managerObject.AddComponent<ScoreManager>();
    }

    private void Awake()
    {
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
        HighScore = PlayerPrefs.GetInt(HighScoreKey, 0);
        ResetRoundStats();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        TrySubscribeToGold();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (isGoldSubscribed && GoldManager.Instance != null)
        {
            GoldManager.Instance.OnGoldChanged -= HandleGoldChanged;
            isGoldSubscribed = false;
        }
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TrySubscribeToGold();
    }

    public void ResetRoundStats()
    {
        RoundScore = 0;
        RoundGoldEarned = 0;
        LittleKillCount = 0;
        NormalKillCount = 0;
        BigKillCount = 0;
        BossKillCount = 0;

        if (GoldManager.Instance != null)
        {
            lastObservedGold = GoldManager.Instance.CurrentGold;
        }
        else
        {
            lastObservedGold = int.MinValue;
        }
    }

    public void RegisterKill(Enemy.ZombieType zombieType)
    {
        RoundScore += GetPointsForZombieType(zombieType);

        switch (zombieType)
        {
            case Enemy.ZombieType.Little:
                LittleKillCount++;
                break;
            case Enemy.ZombieType.Normal:
                NormalKillCount++;
                break;
            case Enemy.ZombieType.Big:
                BigKillCount++;
                break;
            case Enemy.ZombieType.Boss:
                BossKillCount++;
                break;
        }

        if (RoundScore > HighScore)
        {
            HighScore = RoundScore;
            PlayerPrefs.SetInt(HighScoreKey, HighScore);
            PlayerPrefs.Save();
        }
    }

    private int GetPointsForZombieType(Enemy.ZombieType zombieType)
    {
        switch (zombieType)
        {
            case Enemy.ZombieType.Little:
                return littleZombiePoints;
            case Enemy.ZombieType.Normal:
                return normalZombiePoints;
            case Enemy.ZombieType.Big:
                return bigZombiePoints;
            case Enemy.ZombieType.Boss:
                return bossZombiePoints;
            default:
                return 0;
        }
    }

    private void TrySubscribeToGold()
    {
        if (GoldManager.Instance == null)
            return;

        if (!isGoldSubscribed)
        {
            GoldManager.Instance.OnGoldChanged += HandleGoldChanged;
            isGoldSubscribed = true;
        }

        if (lastObservedGold == int.MinValue)
        {
            lastObservedGold = GoldManager.Instance.CurrentGold;
        }
    }

    private void HandleGoldChanged(int currentGold)
    {
        if (lastObservedGold == int.MinValue)
        {
            lastObservedGold = currentGold;
            return;
        }

        int delta = currentGold - lastObservedGold;
        if (delta > 0)
        {
            RoundGoldEarned += delta;
        }

        lastObservedGold = currentGold;
    }
}
