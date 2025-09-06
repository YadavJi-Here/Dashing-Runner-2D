using UnityEngine;
using System.Collections;

public class BombBehavior : MonoBehaviour
{
    [Header("Bomb Physics")]
    public float fuseTime = 3f;
    public float explosionRadius = 3f;
    public int explosionDamage = 50;
    public LayerMask damageableLayers = -1;

    [Header("Visual Effects")]
    public GameObject explosionEffect; // Assign explosion particle effect
    public float flashInterval = 0.2f; // How fast the bomb flashes before exploding

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip tickSound;
    public AudioClip explosionSound;

    private Rigidbody2D rb;
    private Animator bombAnimator;
    private SpriteRenderer spriteRenderer;
    private bool hasExploded = false;
    private bool isArmed = false;
    private BombThrower thrower;

    [Header("Animation")]
    public string idleAnimationState = "BombIdle";
    public string explosionAnimationTrigger = "Explode";

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        bombAnimator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        // Configure rigidbody
        if (rb != null)
        {
            rb.gravityScale = 1f;
            rb.linearDamping = 0.5f; // Some air resistance
        }
    }

    public void Initialize(Vector2 throwDirection, float throwForce, BombThrower bombThrower)
    {
        thrower = bombThrower;

        // Apply throw force
        if (rb != null)
        {
            rb.linearVelocity = throwDirection * throwForce;
        }

        // Start fuse timer
        StartCoroutine(FuseCountdown());

        // Start idle animation
        if (bombAnimator != null)
        {
            bombAnimator.Play(idleAnimationState);
        }

        isArmed = true;
    }

    IEnumerator FuseCountdown()
    {
        float timeRemaining = fuseTime;

        while (timeRemaining > 0 && !hasExploded)
        {
            // Play tick sound
            if (audioSource != null && tickSound != null && timeRemaining > 1f)
            {
                audioSource.PlayOneShot(tickSound);
            }

            // Start flashing when close to explosion
            if (timeRemaining <= 1f)
            {
                StartCoroutine(FlashBomb());
            }

            yield return new WaitForSeconds(0.5f);
            timeRemaining -= 0.5f;
        }

        // Explode
        if (!hasExploded)
        {
            Explode();
        }
    }

    IEnumerator FlashBomb()
    {
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(flashInterval);
            spriteRenderer.color = originalColor;
        }
    }

    void Explode()
    {
        if (hasExploded) return;

        hasExploded = true;

        // Trigger explosion animation
        if (bombAnimator != null)
        {
            bombAnimator.SetTrigger(explosionAnimationTrigger);
        }

        // Play explosion sound
        if (audioSource != null && explosionSound != null)
        {
            audioSource.PlayOneShot(explosionSound);
        }

        // Create explosion effect
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // Apply damage to nearby objects
        ApplyExplosionDamage();

        // Remove from parent (ungroup from player)
        transform.SetParent(null);

        // Destroy after explosion animation
        StartCoroutine(DestroyAfterExplosion());
    }

    void ApplyExplosionDamage()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius, damageableLayers);

        foreach (Collider2D hitCollider in hitColliders)
        {
            // Apply damage to enemies or destructible objects
            IDamageable damageable = hitCollider.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(explosionDamage);
            }

            // Apply knockback to rigidbodies
            Rigidbody2D hitRb = hitCollider.GetComponent<Rigidbody2D>();
            if (hitRb != null)
            {
                Vector2 knockbackDirection = (hitCollider.transform.position - transform.position).normalized;
                float knockbackForce = 10f;
                hitRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
            }
        }
    }

    IEnumerator DestroyAfterExplosion()
    {
        // Wait for explosion animation to finish
        yield return new WaitForSeconds(1f);

        // Destroy the bomb
        Destroy(gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Reduce velocity on impact for more realistic physics
        if (rb != null && isArmed)
        {
            rb.linearVelocity *= 0.7f;
        }

        // Optional: Explode on impact with certain objects
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Explode immediately on enemy contact
            StopAllCoroutines();
            Explode();
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw explosion radius in scene view
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}