using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Settings")]
    public float attackRange = 2f;
    public float attackCooldown = 1.0f;
    public float attackLockDuration = 1.0f; 
    public LayerMask enemyLayer;

    [Header("References")]
    [Header("References")]
    public RaycastHurtbox currentWeapon;
    private Animator animator;
    private PlayerController playerController;

    // State
    private float attackTimer;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
        
        if (animator == null)
        {
            Debug.LogError("PlayerCombat: No Animator found on Player or Children!");
        }
    }

    private void Update()
    {
        // Cooldown management
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }

        CheckForEnemies();
    }

    private void CheckForEnemies()
    {
        // 1. Find enemies in range
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange, enemyLayer);
        
        if (hits.Length > 0)
        {
            // Pick the first valid enemy
            EnemyHealth target = hits[0].GetComponent<EnemyHealth>();
            
            // Check if Alive OR Harvestable
            if (target != null && (!target.IsDead || target.IsHarvestable))
            {
                // 2. Face the enemy (optional but looks better)
                FaceTarget(target.transform.position);

                // 3. Attack if ready
                if (attackTimer <= 0)
                {
                    PerformAttack(target);
                }
            }
            else
            {
                // Debug if we hit something but it wasn't an alive EnemyHealth
                if(target == null) Debug.LogWarning($"PlayerCombat: Hit {hits[0].name} but it has no EnemyHealth script!");
            }
        }
    }

    private void FaceTarget(Vector3 targetPos)
    {
        Vector3 direction = (targetPos - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
        }
    }

    private void PerformAttack(EnemyHealth target)
    {
        // Trigger Animation
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
        
        // Physics Polish: Lock movement & increase mass
        if (playerController != null) StartCoroutine(AttackLockRoutine());

        // Note: Damage is now handled by the WeaponHitbox via Animation Events (StartAttack/EndAttack)

        // Reset cooldown
        attackTimer = attackCooldown;
    }

    private System.Collections.IEnumerator AttackLockRoutine()
    {
        playerController.SetAttackingState(true);
        // Lock for approx swing duration
        yield return new WaitForSeconds(attackLockDuration); 
        playerController.SetAttackingState(false);
    }

    // --- ANIMATION EVENTS ---
    public void ActivateHitbox()
    {
        if (currentWeapon != null)
        {
            currentWeapon.StartAttack();
        }
    }

    public void DeactivateHitbox()
    {
         if (currentWeapon != null)
        {
            currentWeapon.EndAttack();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
