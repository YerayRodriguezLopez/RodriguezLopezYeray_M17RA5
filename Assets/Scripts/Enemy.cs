/*using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float attackCooldown = 2f;

    [Header("Patrol")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float patrolWaitTime = 2f;

    private NavMeshAgent agent;
    private Animator animator;
    private Transform player;

    private float currentHealth;
    private float lastAttackTime;
    private int currentPatrolIndex;
    private float patrolWaitTimer;

    // States
    private enum EnemyState { Idle, Patrol, Chase, Attack, Dead }
    private EnemyState currentState = EnemyState.Idle;

    // Animation hashes
    private int speedHash;
    private int attackHash;
    private int dieHash;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        speedHash = Animator.StringToHash("Speed");
        attackHash = Animator.StringToHash("Attack");
        dieHash = Animator.StringToHash("Die");

        currentHealth = maxHealth;
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (patrolPoints.Length > 0)
        {
            currentState = EnemyState.Patrol;
        }
        else
        {
            currentState = EnemyState.Idle;
        }
    }

    private void Update()
    {
        if (currentState == EnemyState.Dead) return;

        UpdateState();
        UpdateAnimation();
    }

    private void UpdateState()
    {
        float distanceToPlayer = player != null ? Vector3.Distance(transform.position, player.position) : Mathf.Infinity;

        switch (currentState)
        {
            case EnemyState.Idle:
                if (distanceToPlayer <= detectionRange)
                {
                    currentState = EnemyState.Chase;
                }
                break;

            case EnemyState.Patrol:
                Patrol();

                if (distanceToPlayer <= detectionRange)
                {
                    currentState = EnemyState.Chase;
                }
                break;

            case EnemyState.Chase:
                if (distanceToPlayer <= attackRange)
                {
                    currentState = EnemyState.Attack;
                    agent.isStopped = true;
                }
                else if (distanceToPlayer > detectionRange * 1.5f)
                {
                    currentState = EnemyState.Patrol;
                    agent.isStopped = false;
                }
                else
                {
                    ChasePlayer();
                }
                break;

            case EnemyState.Attack:
                if (distanceToPlayer > attackRange)
                {
                    currentState = EnemyState.Chase;
                    agent.isStopped = false;
                }
                else
                {
                    AttackPlayer();
                }
                break;
        }
    }

    private void Patrol()
    {
        if (patrolPoints.Length == 0) return;

        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            patrolWaitTimer += Time.deltaTime;

            if (patrolWaitTimer >= patrolWaitTime)
            {
                patrolWaitTimer = 0f;
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                agent.SetDestination(patrolPoints[currentPatrolIndex].position);
            }
        }
    }

    private void ChasePlayer()
    {
        if (player != null)
        {
            agent.SetDestination(player.position);
        }
    }

    private void AttackPlayer()
    {
        if (player != null)
        {
            transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
        }

        if (Time.time - lastAttackTime >= attackCooldown)
        {
            lastAttackTime = Time.time;
            animator.SetTrigger(attackHash);

            // El daño se aplica en un evento de animación
        }
    }

    // Llamado por Animation Event
    public void DealDamage()
    {
        if (player != null && Vector3.Distance(transform.position, player.position) <= attackRange)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage(attackDamage);
            }
        }
    }

    private void UpdateAnimation()
    {
        float speed = agent.velocity.magnitude;
        animator.SetFloat(speedHash, speed, 0.1f, Time.deltaTime);
    }

    public void TakeDamage(float damage)
    {
        if (currentState == EnemyState.Dead) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Agregar animación de hit si existe
            currentState = EnemyState.Chase;
        }
    }

    private void Die()
    {
        currentState = EnemyState.Dead;
        animator.SetTrigger(dieHash);

        agent.isStopped = true;
        agent.enabled = false;

        // Desactivar collider para que no bloquee
        GetComponent<Collider>().enabled = false;

        Destroy(gameObject, 5f);
    }

    private void OnDrawGizmosSelected()
    {
        // Visualizar rangos
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}*/