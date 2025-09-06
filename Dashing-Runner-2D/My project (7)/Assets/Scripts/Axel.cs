using UnityEngine;

public class Axel: MonoBehaviour
{
    [Header("Components")]
    private Animator animator;
    private Rigidbody2D rb;
    private Vector3 originalScale;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpForce = 15f;

    [Header("Ground Detection")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.3f;
    public LayerMask groundLayer;
    private bool isGrounded;

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
    }

    void Update()
    {
        HandleInput();
        HandleMovement();
        UpdateAnimations();
        CheckGrounded();
        PreventUnwantedScaling();
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
            if (Input.GetMouseButtonDown(0))
                PerformLeftPunch();
            if (Input.GetMouseButtonUp(1))
                PerformRightPunch();
            if (Input.GetKeyDown(KeyCode.C))
                PerformKick();
            if (Input.GetKeyDown(KeyCode.V))
                PerformUppercut();
        }
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

            if (horizontalInput > 0.1f)
                transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
            else if (horizontalInput < -0.1f)
                transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
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

        StartCoroutine(DisableActionsTemporarily(0.3f));
    }

    void PerformLeftPunch()
    {
        SafeSetTrigger(LEFT_PUNCH_TRIGGER);
        SafeSetBool(IS_LEFT_PUNCH, true);
        StartCoroutine(DisableActionsTemporarily(0.3f));
        StartCoroutine(ResetBoolAfterTime(IS_LEFT_PUNCH, 0.5f));
    }

    void PerformRightPunch()
    {
        SafeSetTrigger(RIGHT_PUNCH_TRIGGER);
        SafeSetBool(IS_RIGHT_PUNCH, true);
        StartCoroutine(DisableActionsTemporarily(0.3f));
        StartCoroutine(ResetBoolAfterTime(IS_RIGHT_PUNCH, 0.5f));
    }

    void PerformKick()
    {
        SafeSetTrigger(KICK_TRIGGER);
        SafeSetBool(IS_KICKING, true);
        StartCoroutine(DisableActionsTemporarily(0.4f));
        StartCoroutine(ResetBoolAfterTime(IS_KICKING, 0.6f));
    }

    void PerformUppercut()
    {
        SafeSetTrigger(UPPERCUT_TRIGGER);
        SafeSetBool(IS_UPPERCUT, true);
        StartCoroutine(DisableActionsTemporarily(0.5f));
        StartCoroutine(ResetBoolAfterTime(IS_UPPERCUT, 0.7f));
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
}