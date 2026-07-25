using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    public Slider healthSlider;
    public Transform carTarget;

    void Start()
    {
        if (healthSlider != null)
        {
            healthSlider.minValue = 0f;
            healthSlider.maxValue = 1f;
        }
    }

    void Update()
    {
        if (carTarget == null || healthSlider == null)
            return;

        CarHealth carHealth = carTarget.GetComponent<CarHealth>();
        if (carHealth != null)
        {
            healthSlider.value = carHealth.GetHealthPercent();
        }
    }
}
