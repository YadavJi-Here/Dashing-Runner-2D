using UnityEngine;

// Interface for objects that can take damage from explosions
public interface IDamageable
{
    void TakeDamage(int damage);
}

// Example implementation for enemies
public class Enemy : MonoBehaviour, IDamageable
{
    [Header("Health")]
    public int maxHealth = 100;
    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // Add damage effects here (flash, sound, etc.)

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // Add death effects
        Destroy(gameObject);
    }
}

// Example implementation for destructible objects
public class DestructibleObject : MonoBehaviour, IDamageable
{
    [Header("Destruction")]
    public int health = 50;
    public GameObject destroyedVersion; // Optional broken version

    public void TakeDamage(int damage)
    {
        health -= damage;

        if (health <= 0)
        {
            Destroy();
        }
    }

    void Destroy()
    {
        if (destroyedVersion != null)
        {
            Instantiate(destroyedVersion, transform.position, transform.rotation);
        }

        Destroy(gameObject);
    }
}