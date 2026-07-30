using UnityEngine;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(TMP_Text))]
public class GoldUI : MonoBehaviour
{
    public TMP_Text goldText;
    public Image goldImage;

    private bool subscribed;

    void Start()
    {
        if (goldText == null)
        {
            goldText = GetComponent<TMP_Text>();
        }
        
        TrySubscribe();
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
        if (goldText == null)
        {
            goldText = GetComponent<TMP_Text>();
        }

        if (GoldManager.Instance == null)
            return;

        if (!subscribed)
        {
            GoldManager.Instance.OnGoldChanged += UpdateGoldText;
            subscribed = true;
        }
        
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
