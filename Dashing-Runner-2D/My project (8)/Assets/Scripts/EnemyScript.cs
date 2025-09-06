using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Patrol Settings")]
    public Transform pointA;
    public Transform pointB;
    public float patrolSpeed = 2f;
    private Transform currentTarget;

    [Header("Chase Settings")]
    public float chaseRange = 5f;
    public float attackRange = 1.2f;
    public float chaseSpeed = 3f;

    [Header("Attack Settings")]
    public int attackDamage = 10;
    public float attackCooldown = 1.5f;
    private float lastAttackTime;

    private Animator animator;
    private Transform player;
    private bool isChasing = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        currentTarget = pointA;
    }

    void Update()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            AttackPlayer();
        }
        else if (distanceToPlayer <= chaseRange)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    void Patrol()
    {
        isChasing = false;
        animator.SetBool("isWalking", true);
        animator.SetBool("isRunning", false);

        transform.position = Vector2.MoveTowards(transform.position, currentTarget.position, patrolSpeed * Time.deltaTime);

        // Flip when reaching patrol points
        if (Vector2.Distance(transform.position, currentTarget.position) < 0.2f)
        {
            currentTarget = (currentTarget == pointA) ? pointB : pointA;
            Flip();
        }
    }

    void ChasePlayer()
    {
        isChasing = true;
        animator.SetBool("isWalking", false);
        animator.SetBool("isRunning", true);

        transform.position = Vector2.MoveTowards(transform.position, player.position, chaseSpeed * Time.deltaTime);

        // Face player
        if (player.position.x > transform.position.x && transform.localScale.x < 0)
        {
            Flip();
        }
        else if (player.position.x < transform.position.x && transform.localScale.x > 0)
        {
            Flip();
        }
    }

    void AttackPlayer()
    {
        animator.SetBool("isWalking", false);
        animator.SetBool("isRunning", false);

        if (Time.time - lastAttackTime >= attackCooldown)
        {
            animator.SetTrigger("Attack");

            // Apply damage
            AxelHealth axelHealth = player.GetComponent<AxelHealth>();
            if (axelHealth != null)
            {
                axelHealth.TakeDamage(attackDamage);
            }

            lastAttackTime = Time.time;
        }
    }

    void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}

