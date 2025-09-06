using UnityEngine;
using static UnityEditor.IMGUI.Controls.PrimitiveBoundsHandle;

public class AxelHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    private int currentHealth;

    private Animator animator;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("Axel takes damage! Current health: " + currentHealth);

        if (animator != null)
        {
            animator.SetTrigger("Hurt"); // Play Hurt animation if available
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Axel died!");
        if (animator != null)
        {
            animator.SetTrigger("Die"); // Play Death animation if available
        }

        // Disable Axel movement script when dead
        //GetComponent<Axel>()?.enabled = false;
    }
}


