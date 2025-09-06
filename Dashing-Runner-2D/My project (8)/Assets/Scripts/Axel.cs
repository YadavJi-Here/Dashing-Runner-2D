using UnityEngine;

public class FixedJumpingController : MonoBehaviour
{
    [Header("Components")]
    private Animator animator;
    private Rigidbody2D rb;
    private Vector3 originalScale;

    [Header("Audio Components")]
    public AudioSource musicAudioSource;
    public AudioSource sfxAudioSource;

    [Header("Audio Clips")]
    public AudioClip backgroundMusic;
    public AudioClip punchWhooshSound;
    public AudioClip punchHitSound;
    public AudioClip kickWhooshSound;
    public AudioClip kickHitSound;
    public AudioClip jumpSound;
    public AudioClip uppercutSound;

    [Header("Audio Settings")]
    [Range(0f, 1f)]
    public float musicVolume = 0.5f;
    [Range(0f, 1f)]
    public float sfxVolume = 0.7f;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpForce = 15f;

    [Header("Ground Detection")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.3f;
    public LayerMask groundLayer;
    private bool isGrounded;

    [Header("Swipe Detection")]
    public float minSwipeDistance = 50f;
    public float maxSwipeTime = 1f;
    private Vector2 startTouchPosition;
    private Vector2 endTouchPosition;
    private float startTime;
    private bool isMousePressed = false;

    [Header("Animation Parameters")]
    private const string IS_WALKING = "isWalking";
    private const string IS_RUNNING = "isRunning";
    private const string IS_JUMPING = "isJumping";
    private const string IS_KICKING = "isKicking";
    private const string IS_LEFT_PUNCH = "isLeftPunch";
    private const string IS_RIGHT_PUNCH = "isRightPunch";
    private const string IS_UPPERCUT = "isUpperCut";
    private const string IS_DASH = "isDash";
    private const string JUMP_TRIGGER = "Jump";
    private const string KICK_TRIGGER = "Kick";
    private const string LEFT_PUNCH_TRIGGER = "LeftPunch";
    private const string RIGHT_PUNCH_TRIGGER = "RightPunch";
    private const string UPPERCUT_TRIGGER = "UpperCut";
    private const string DASH_TRIGGER = "Dash";

    [Header("Input")]
    private float horizontalInput;
    private bool isRunning;
    private bool canPerformActions = true;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        originalScale = transform.localScale;

        if (animator != null)
        {
            animator.applyRootMotion = false;
        }

        if (rb != null)
        {
            rb.gravityScale = 3f;
            rb.freezeRotation = true;
        }

        CreateGroundCheck();
        SetupAudio();
        PlayBackgroundMusic();
    }

    void SetupAudio()
    {
        // Create Music Audio Source if not assigned
        if (musicAudioSource == null)
        {
            GameObject musicObj = new GameObject("MusicAudioSource");
            musicObj.transform.SetParent(transform);
            musicAudioSource = musicObj.AddComponent<AudioSource>();
        }

        // Create SFX Audio Source if not assigned
        if (sfxAudioSource == null)
        {
            GameObject sfxObj = new GameObject("SFXAudioSource");
            sfxObj.transform.SetParent(transform);
            sfxAudioSource = sfxObj.AddComponent<AudioSource>();
        }

        // Configure Music Audio Source
        musicAudioSource.loop = true;
        musicAudioSource.playOnAwake = false;
        musicAudioSource.volume = musicVolume;

        // Configure SFX Audio Source
        sfxAudioSource.loop = false;
        sfxAudioSource.playOnAwake = false;
        sfxAudioSource.volume = sfxVolume;
    }

    void PlayBackgroundMusic()
    {
        if (musicAudioSource != null && backgroundMusic != null)
        {
            musicAudioSource.clip = backgroundMusic;
            musicAudioSource.Play();
        }
    }

    void PlaySFX(AudioClip clip)
    {
        if (sfxAudioSource != null && clip != null)
        {
            sfxAudioSource.PlayOneShot(clip);
        }
    }

    void Update()
    {
        HandleInput();
        CheckGrounded();
        HandleMovement();
        UpdateAnimations();
        PreventUnwantedScaling();
        UpdateAudioVolumes();
    }

    void UpdateAudioVolumes()
    {
        if (musicAudioSource != null)
            musicAudioSource.volume = musicVolume;
        if (sfxAudioSource != null)
            sfxAudioSource.volume = sfxVolume;
    }

    void PreventUnwantedScaling()
    {
        if (transform.localScale.y != originalScale.y || transform.localScale.z != originalScale.z)
        {
            float currentXScale = transform.localScale.x;
            transform.localScale = new Vector3(currentXScale, originalScale.y, originalScale.z);
        }
    }

    void HandleInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            AttemptJump();
        }

        if (canPerformActions)
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                PerformUppercut();
            }

            HandleSwipeInput();
        }
    }

    void HandleSwipeInput()
    {
        if (Input.GetMouseButtonDown(0) && !isMousePressed)
        {
            startTouchPosition = Input.mousePosition;
            startTime = Time.time;
            isMousePressed = true;
        }

        if (Input.GetMouseButtonUp(0) && isMousePressed)
        {
            endTouchPosition = Input.mousePosition;
            float swipeTime = Time.time - startTime;
            isMousePressed = false;

            if (swipeTime <= maxSwipeTime)
            {
                Vector2 swipeDirection = endTouchPosition - startTouchPosition;
                float swipeDistance = swipeDirection.magnitude;

                if (swipeDistance >= minSwipeDistance)
                {
                    swipeDirection.Normalize();

                    if (Mathf.Abs(swipeDirection.x) > Mathf.Abs(swipeDirection.y))
                    {
                        if (swipeDirection.x > 0.5f) // Right swipe
                        {
                            FaceRight();
                            PerformRightPunch();
                        }
                        else if (swipeDirection.x < -0.5f) // Left swipe
                        {
                            FaceLeft();
                            PerformLeftPunch();
                        }
                    }
                    else
                    {
                        if (swipeDirection.y < -0.5f) // Down swipe
                        {
                            PerformKick();
                        }
                    }
                }
            }
        }
    }

    void FaceRight()
    {
        transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
    }

    void FaceLeft()
    {
        transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
    }

    void CheckGrounded()
    {
        if (groundCheck == null)
        {
            return;
        }

        bool layerCheck = false;
        if (groundLayer != 0)
        {
            layerCheck = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }

        bool tagCheck = false;
        Collider2D groundCollider = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius);
        if (groundCollider != null)
        {
            tagCheck = groundCollider.CompareTag("Ground");
        }

        RaycastHit2D raycastHit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckRadius + 0.1f);
        bool raycastCheck = raycastHit.collider != null && raycastHit.collider.CompareTag("Ground");

        isGrounded = layerCheck || tagCheck || raycastCheck;
    }

    void HandleMovement()
    {
        if (rb != null)
        {
            float currentSpeed = isRunning ? runSpeed : moveSpeed;
            float moveX = horizontalInput * currentSpeed;
            rb.linearVelocity = new Vector2(moveX, rb.linearVelocity.y);

            if (canPerformActions)
            {
                if (horizontalInput > 0.1f)
                    transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
                else if (horizontalInput < -0.1f)
                    transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
            }
        }
    }

    void UpdateAnimations()
    {
        if (animator == null) return;

        bool isMoving = Mathf.Abs(horizontalInput) > 0.1f;

        try
        {
            if (isMoving && isRunning)
            {
                SafeSetBool(IS_WALKING, false);
                SafeSetBool(IS_RUNNING, true);
            }
            else if (isMoving)
            {
                SafeSetBool(IS_WALKING, true);
                SafeSetBool(IS_RUNNING, false);
            }
            else
            {
                SafeSetBool(IS_WALKING, false);
                SafeSetBool(IS_RUNNING, false);
            }

            SafeSetBool(IS_JUMPING, !isGrounded);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Animation error: {e.Message}");
        }
    }

    void AttemptJump()
    {
        if (rb == null)
        {
            return;
        }

        if (!isGrounded)
        {
            return;
        }

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        SafeSetTrigger(JUMP_TRIGGER);

        // Play jump sound
        PlaySFX(jumpSound);

        StartCoroutine(DisableActionsTemporarily(0.3f));
    }

    void PerformLeftPunch()
    {
        SafeSetTrigger(LEFT_PUNCH_TRIGGER);
        SafeSetBool(IS_LEFT_PUNCH, true);

        // Play punch whoosh sound immediately
        PlaySFX(punchWhooshSound);

        // Play punch hit sound with slight delay for impact
        StartCoroutine(PlayDelayedSFX(punchHitSound, 0.15f));

        StartCoroutine(DisableActionsTemporarily(0.3f));
        StartCoroutine(ResetBoolAfterTime(IS_LEFT_PUNCH, 0.5f));
        Debug.Log("Left Punch performed!");
    }

    void PerformRightPunch()
    {
        SafeSetTrigger(RIGHT_PUNCH_TRIGGER);
        SafeSetBool(IS_RIGHT_PUNCH, true);

        // Play punch whoosh sound immediately
        PlaySFX(punchWhooshSound);

        // Play punch hit sound with slight delay for impact
        StartCoroutine(PlayDelayedSFX(punchHitSound, 0.15f));

        StartCoroutine(DisableActionsTemporarily(0.3f));
        StartCoroutine(ResetBoolAfterTime(IS_RIGHT_PUNCH, 0.5f));
        Debug.Log("Right Punch performed!");
    }

    void PerformKick()
    {
        SafeSetTrigger(KICK_TRIGGER);
        SafeSetBool(IS_KICKING, true);

        // Play kick whoosh sound immediately
        PlaySFX(kickWhooshSound);

        // Play kick hit sound with slight delay for impact
        StartCoroutine(PlayDelayedSFX(kickHitSound, 0.2f));

        StartCoroutine(DisableActionsTemporarily(0.4f));
        StartCoroutine(ResetBoolAfterTime(IS_KICKING, 0.6f));
        Debug.Log("Kick performed!");
    }

    void PerformUppercut()
    {
        SafeSetTrigger(UPPERCUT_TRIGGER);
        SafeSetBool(IS_UPPERCUT, true);

        // Play uppercut sound
        PlaySFX(uppercutSound);

        StartCoroutine(DisableActionsTemporarily(0.5f));
        StartCoroutine(ResetBoolAfterTime(IS_UPPERCUT, 0.7f));
        Debug.Log("Uppercut performed!");
    }

    System.Collections.IEnumerator PlayDelayedSFX(AudioClip clip, float delay)
    {
        yield return new WaitForSeconds(delay);
        PlaySFX(clip);
    }

    void SafeSetBool(string paramName, bool value)
    {
        if (animator != null)
        {
            try
            {
                animator.SetBool(paramName, value);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Bool parameter '{paramName}' not found: {e.Message}");
            }
        }
    }

    void SafeSetTrigger(string paramName)
    {
        if (animator != null)
        {
            try
            {
                animator.SetTrigger(paramName);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Trigger parameter '{paramName}' not found: {e.Message}");
            }
        }
    }

    void CreateGroundCheck()
    {
        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.SetParent(transform);
            groundCheckObj.transform.localPosition = new Vector3(0, -0.8f, 0);
            groundCheck = groundCheckObj.transform;
        }
    }

    System.Collections.IEnumerator DisableActionsTemporarily(float duration)
    {
        canPerformActions = false;
        yield return new WaitForSeconds(duration);
        canPerformActions = true;
    }

    System.Collections.IEnumerator ResetBoolAfterTime(string paramName, float duration)
    {
        yield return new WaitForSeconds(duration);
        SafeSetBool(paramName, false);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, Vector2.down * (groundCheckRadius + 0.1f));
    }

    // Public methods for controlling audio from outside
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicAudioSource != null)
            musicAudioSource.volume = musicVolume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxAudioSource != null)
            sfxAudioSource.volume = sfxVolume;
    }

    public void StopBackgroundMusic()
    {
        if (musicAudioSource != null)
            musicAudioSource.Stop();
    }

    public void PlayBackgroundMusicAgain()
    {
        PlayBackgroundMusic();
    }
}
