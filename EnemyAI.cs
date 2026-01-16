using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class EnemyAI : MonoBehaviour
{
    public enum State
    {
        Wander,
        Chase,
        Attack
    }

    [Header("Settings")]
    public float wanderRadius = 20f;
    public float detectionRadius = 10f;
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;
    public float wanderSpeed = 2f;
    public float chaseSpeed = 5f;
    public LayerMask targetMask; // Includes Player and Gates

    [Header("Phase Settings")]
    public bool startsPassive = true;

    [Header("Debug")]
    public State currentState;
    public Transform currentTarget;

    private NavMeshAgent agent;
    private Animator animator;
    private float attackTimer;
    private bool isPassive;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        currentState = State.Wander;
        attackTimer = 0f;
        isPassive = startsPassive;

        CampfireManager.OnFortressSecureEvent += OnFortressSecure;
        
        agent.speed = wanderSpeed;
        SetRandomDestination();
    }

    private void Update()
    {
        if (!agent.isOnNavMesh) return;

        // Animation
        animator.SetFloat("Speed", agent.velocity.magnitude);

        // Cooldowns
        if (attackTimer > 0) attackTimer -= Time.deltaTime;

        // State Machine
        switch (currentState)
        {
            case State.Wander:
                HandleWander();
                if (!isPassive) CheckForDetection();
                break;

            case State.Chase:
                HandleChase();
                break;

            case State.Attack:
                HandleAttack();
                break;
        }
    }

    private void OnDestroy()
    {
        CampfireManager.OnFortressSecureEvent -= OnFortressSecure;
    }

    private void OnFortressSecure()
    {
        isPassive = false;
    }

    private void HandleWander()
    {
        // 1. Check if reached destination
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            SetRandomDestination();
        }
    }

    private void SetRandomDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, 1))
        {
            agent.SetDestination(hit.position);
        }
    }

    private void CheckForDetection()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, targetMask);
        
        Collider bestTarget = null;
        float closeDist = Mathf.Infinity;

        foreach(var hit in hits)
        {
            float d = Vector3.Distance(transform.position, hit.transform.position);
            if(d < closeDist)
            {
                closeDist = d;
                bestTarget = hit;
            }
        }

        if (bestTarget != null)
        {
            currentTarget = bestTarget.transform;
            currentState = State.Chase;
            agent.speed = chaseSpeed;
        }
    }

    private void HandleChase()
    {
        if (currentTarget == null)
        {
            ReturnToWander();
            return;
        }

        float dist = Vector3.Distance(transform.position, currentTarget.position);

        // Optimization: Only update path if target moves significantly? 
        // For now, simple update is robust.
        agent.SetDestination(currentTarget.position);

        if (dist <= attackRange)
        {
            currentState = State.Attack;
            agent.ResetPath();
        }
        else if (dist > detectionRadius * 1.5f) // Lost usage
        {
            ReturnToWander();
        }
    }

    private void HandleAttack()
    {
        if (currentTarget == null)
        {
            ReturnToWander();
            return;
        }

        float dist = Vector3.Distance(transform.position, currentTarget.position);
        if (dist > attackRange)
        {
            currentState = State.Chase;
            return;
        }

        // Face Target
        Vector3 dir = (currentTarget.position - transform.position).normalized;
        dir.y = 0;
        if(dir != Vector3.zero) 
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 5f);
        
        // Attack
        if (attackTimer <= 0)
        {
            animator.SetTrigger("Attack");
            
            // Try damage Player
            var playerHealth = currentTarget.GetComponent<PlayerHealth>();
            if(playerHealth) playerHealth.TakeDamage(10f);

            // Try damage Gate/Building (If you have a script for that, e.g. BuildingHealth)
            // var building = currentTarget.GetComponent<BuildingHealth>();
            // if(building) building.TakeDamage(10f);

            attackTimer = attackCooldown;
        }
    }

    private void ReturnToWander()
    {
        currentState = State.Wander;
        currentTarget = null;
        agent.speed = wanderSpeed;
        SetRandomDestination();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, wanderRadius);
    }
}
