using UnityEngine;
using System.Collections;

public class BombThrower : MonoBehaviour
{
    [Header("Bomb Settings")]
    public GameObject bombPrefab;
    public Transform bombSpawnPoint; // Position at character's waist/belt
    public int maxBombs = 3;
    public float throwForce = 10f;
    public float throwAngle = 45f;
    public float bombCooldown = 1f;

    [Header("UI References")]
    public UnityEngine.UI.Text bombCountText; // Optional UI display

    private int currentBombCount;
    private bool canThrow = true;
    private Animator playerAnimator;

    [Header("Animation")]
    public string throwAnimationTrigger = "ThrowBomb";

    void Start()
    {
        currentBombCount = maxBombs;
        playerAnimator = GetComponent<Animator>();

        // Create bomb spawn point if not assigned
        if (bombSpawnPoint == null)
        {
            GameObject spawnPoint = new GameObject("BombSpawnPoint");
            spawnPoint.transform.SetParent(transform);
            spawnPoint.transform.localPosition = new Vector3(0.5f, 0f, 0f); // At waist level
            bombSpawnPoint = spawnPoint.transform;
        }

        UpdateBombUI();
    }

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        // Throw bomb on key press (B key or right mouse button)
        if (Input.GetKeyDown(KeyCode.B) || Input.GetMouseButtonDown(1))
        {
            ThrowBomb();
        }
    }

    public void ThrowBomb()
    {
        if (!CanThrowBomb())
        {
            return;
        }

        // Trigger throw animation
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger(throwAnimationTrigger);
        }

        // Start throw coroutine with slight delay for animation
        StartCoroutine(ThrowBombCoroutine());
    }

    IEnumerator ThrowBombCoroutine()
    {
        // Small delay to sync with animation
        yield return new WaitForSeconds(0.2f);

        // Create bomb
        GameObject bomb = Instantiate(bombPrefab, bombSpawnPoint.position, Quaternion.identity);

        // Set bomb as child of player (grouped in hierarchy)
        bomb.transform.SetParent(transform);

        // Get bomb component and initialize
        BombBehavior bombScript = bomb.GetComponent<BombBehavior>();
        if (bombScript != null)
        {
            Vector2 throwDirection = CalculateThrowDirection();
            bombScript.Initialize(throwDirection, throwForce, this);
        }

        // Decrease bomb count
        currentBombCount--;
        UpdateBombUI();

        // Apply cooldown
        canThrow = false;
        yield return new WaitForSeconds(bombCooldown);
        canThrow = true;
    }

    Vector2 CalculateThrowDirection()
    {
        // Calculate throw direction based on player facing direction and angle
        float facingDirection = transform.localScale.x > 0 ? 1f : -1f;
        float angleInRadians = throwAngle * Mathf.Deg2Rad;

        Vector2 direction = new Vector2(
            facingDirection * Mathf.Cos(angleInRadians),
            Mathf.Sin(angleInRadians)
        );

        return direction.normalized;
    }

    bool CanThrowBomb()
    {
        return currentBombCount > 0 && canThrow;
    }

    public void RestockBombs()
    {
        currentBombCount = maxBombs;
        UpdateBombUI();
    }

    public void AddBombs(int amount)
    {
        currentBombCount = Mathf.Min(currentBombCount + amount, maxBombs);
        UpdateBombUI();
    }

    void UpdateBombUI()
    {
        if (bombCountText != null)
        {
            bombCountText.text = $"Bombs: {currentBombCount}/{maxBombs}";
        }
    }

    public int GetBombCount()
    {
        return currentBombCount;
    }
}