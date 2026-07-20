using UnityEngine;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(TMP_Text))]
public class GoldUI : MonoBehaviour
{
    private static GoldUI _instance;
    public TMP_Text goldText;
    public Image goldImage;

    private bool subscribed;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        if (goldImage != null)
        {
            DontDestroyOnLoad(goldImage.gameObject);
        }

        if (goldText == null)
        {
            goldText = GetComponent<TMP_Text>();
        }
    }

    void OnEnable()
    {
        TrySubscribe();
    }

    void Update()
    {
        if (!subscribed)
        {
            TrySubscribe();
        }
    }

    void OnDisable()
    {
        if (subscribed && GoldManager.Instance != null)
        {
            GoldManager.Instance.OnGoldChanged -= UpdateGoldText;
        }

        subscribed = false;
    }

    void TrySubscribe()
    {
        if (GoldManager.Instance == null)
            return;

        GoldManager.Instance.OnGoldChanged += UpdateGoldText;
        subscribed = true;
        UpdateGoldText(GoldManager.Instance.CurrentGold);
    }

    void UpdateGoldText(int amount)
    {
        if (goldText != null)
        {
            goldText.text = amount.ToString();
        }
    }
}
