using UnityEngine;
using UnityEngine.AI;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 50f;
    public float currentHealth;
    
    private Animator animator;
    private NavMeshAgent agent;
    private EnemyAI enemyAI;
    private Collider enemyCollider;
    private CombatFeedback combatFeedback;

    public GameObject[] meatPrefabs;
    public float explosionForce = 300f; // NEW: Exposed for tuning
    
    // Harvesting Variables
    private bool isHarvestable = false;
    private int harvestHitsLeft = 2;

    public bool IsDead => currentHealth <= 0;
    public bool IsHarvestable => isHarvestable;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        enemyAI = GetComponent<EnemyAI>();
        enemyCollider = GetComponent<Collider>();
        combatFeedback = GetComponent<CombatFeedback>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        // 1. HARVESTING LOGIC (Post-Death)
        if (isHarvestable)
        {
            harvestHitsLeft--;
            Debug.Log($"Harvesting Hit! Left: {harvestHitsLeft}");
            // Optional: Play chop sound / blood effect
            
            if (harvestHitsLeft <= 0)
            {
                Harvest();
            }
            return;
        }

        // 2. NORMAL COMBAT LOGIC
        if (IsDead) return;

        currentHealth -= amount;
        Debug.Log($"Enemy took {amount} damage. HP: {currentHealth}");

        if (combatFeedback != null) combatFeedback.OnHit();

        // Optional: Trigger "Hit" animation
        // animator.SetTrigger("Hit");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Harvest()
    {
        isHarvestable = false;
        
        // Spawn 5-8 meat
        int meatCount = Random.Range(5, 9);
        if (meatPrefabs != null && meatPrefabs.Length > 0)
        {
            for (int i = 0; i < meatCount; i++)
            {
                // Pick a random steak variation
                GameObject selectedPrefab = meatPrefabs[Random.Range(0, meatPrefabs.Length)];
                LootSpawner.SpawnSingle(selectedPrefab, transform.position, explosionForce);
            }
        }
        
        Destroy(gameObject);
    }

    private void Die()
    {
        animator.SetTrigger("Death"); // Ensure you have a "Death" trigger in Animator
        
        // Disable AI logic
        if (enemyAI != null) enemyAI.enabled = false;
        if (agent != null) agent.enabled = false;
        
        // Disable Collider so player can't hit it anymore
        // CHANGED: Keep collider enabled so player can hit the corpse to harvest
        // if (enemyCollider != null) enemyCollider.enabled = false;

        // Enable Harvest State
        isHarvestable = true;

        // Destroy after a delay if not harvested?
        // Destroy(gameObject, 60f); // Longer decay time
    }
}
