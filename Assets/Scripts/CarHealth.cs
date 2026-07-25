using UnityEngine;
using UnityEngine.SceneManagement;

public class CarHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;

    void Start()
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
