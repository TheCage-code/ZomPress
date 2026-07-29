using UnityEngine;
using UnityEngine.SceneManagement;

public class CarHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    [SerializeField] private int carDamage = 100;
    public int CarDamage => carDamage; // Getter for zombie damage scaling
    private float currentHealth;

    void Start()
    {
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
        Destroy(gameObject);
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
