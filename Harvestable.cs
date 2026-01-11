using UnityEngine;
using System.Collections;

public class Harvestable : MonoBehaviour
{
    public enum ResourceType { Wood, Stone }

    [Header("Stats")]
    public ResourceType type = ResourceType.Wood;
    [Tooltip("Total hits required to break the tree")]
    public int hitsToBreak = 3; 

    [Header("Timing")]
    [Tooltip("Delay before the FIRST hit (matches animation windup)")]
    public float windupTime = 0.4f; 
    [Tooltip("Time between subsequent hits")]
    public float chopInterval = 1.0f; 

    [Header("Loot")]
    public GameObject resourcePrefab; 
    public int resourceAmount = 3; 
    public float explosionForce = 300f;

    [Header("Visuals")]
    public GameObject visualModel; 

    private int currentHealth;
    private bool isPlayerInZone = false;
    private PlayerCollector currentChopper;
    
    // THE SAFETY LOCK 🔒 (Prevents Double Hits)
    private Coroutine choppingRoutine; 

    void Start()
    {
        currentHealth = hitsToBreak;
        // Safety: Ensure we never have 0 interval (causes instant death)
        if (chopInterval < 0.1f) chopInterval = 1.0f; 
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 1. FILTER: Ignore Magnets/Sensors
            if (other.isTrigger) return; 

            // 2. LOCK CHECK: If chopping already, ignore this collider
            if (choppingRoutine != null) return;

            currentChopper = other.GetComponent<PlayerCollector>();
            if (currentChopper != null)
            {
                isPlayerInZone = true;
                currentChopper.SetChoppingState(true);
                
                // 3. START & SAVE the routine
                choppingRoutine = StartCoroutine(ChopLoop());
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.isTrigger) return; // Ignore magnets leaving

            isPlayerInZone = false;
            
            // Clean up
            if (choppingRoutine != null)
            {
                StopCoroutine(choppingRoutine);
                choppingRoutine = null;
            }
            
            if (currentChopper != null)
            {
                currentChopper.SetChoppingState(false);
                currentChopper = null;
            }
        }
    }

    IEnumerator ChopLoop()
    {
        // STEP 1: Fast First Hit (Matches the first swing animation)
        yield return new WaitForSeconds(windupTime);
        if(isPlayerInZone) TakeDamage();

        // STEP 2: Rhythm Hits (Matches the looping animation)
        while (isPlayerInZone && currentHealth > 0)
        {
            yield return new WaitForSeconds(chopInterval);
            if(isPlayerInZone) TakeDamage();
        }
    }

    void TakeDamage()
    {
        currentHealth--;
        Debug.Log($"Chop! Health Remaining: {currentHealth}");

        // Wobble Effect
        if (visualModel != null) StartCoroutine(ShakeVisual());

        if (currentHealth <= 0)
        {
            Harvest();
        }
    }

    void Harvest()
    {
        if (resourcePrefab != null)
        {
            for (int i = 0; i < resourceAmount; i++)
            {
                Vector3 spawnPos = transform.position + Vector3.up * 1.5f + Random.insideUnitSphere * 0.5f;
                GameObject loot = Instantiate(resourcePrefab, spawnPos, Quaternion.identity);

                if(loot.TryGetComponent(out Rigidbody rb))
                {
                    rb.AddForce((Random.insideUnitSphere + Vector3.up).normalized * explosionForce);
                    rb.AddTorque(Random.insideUnitSphere * explosionForce);
                }
            }
        }

        if (currentChopper != null) currentChopper.SetChoppingState(false);
        Destroy(gameObject);
    }

    IEnumerator ShakeVisual()
    {
        Vector3 originalScale = visualModel.transform.localScale;
        visualModel.transform.localScale = originalScale * 1.2f;
        yield return new WaitForSeconds(0.1f);
        visualModel.transform.localScale = originalScale;
    }
}