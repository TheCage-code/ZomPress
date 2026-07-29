using UnityEngine;
using UnityEngine.SceneManagement;

public class CarHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    private const int DefaultCarDamage = 90;
    [SerializeField] private int carDamage = 90;
    [SerializeField] private int carLevel = 0;
    public int CarDamage => carDamage; // Getter for zombie damage scaling
    public int CarLevel => carLevel;
    private float currentHealth;

    void Start()
    {
        if (carLevel <= 0)
        {
            carDamage = DefaultCarDamage;
        }

        currentHealth = maxHealth;
    }

    public void SetMaxHealth(float newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = maxHealth;
    }

    public void SetCarDamage(int damage)
    {
        carDamage = damage;
    }

    public void SetCarLevel(int level)
    {
        carLevel = Mathf.Clamp(level, 0, 9);
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0f);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.LogWarning("Car destroyed! Game Over!");
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public float GetHealth()
    {
        return currentHealth;
    }

    public float GetMaxHealth()
    {
        return maxHealth;
    }

    public float GetHealthPercent()
    {
        return currentHealth / maxHealth;
    }
}
