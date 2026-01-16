using UnityEngine;
using System.Collections;

public class Harvestable : MonoBehaviour
{
    public enum ResourceType { Wood, Stone, Meat }

    [Header("Stats")]
    public ResourceType type = ResourceType.Wood;
    [Tooltip("Total hits required to break the tree")]
    public int hitsToBreak = 3; 

    [Header("Loot")]
    public GameObject resourcePrefab; 
    public int resourceAmount = 3; 
    public float explosionForce = 300f;

    [Header("Visuals")]
    public GameObject visualModel; 
    
    private int currentHits;
    private CombatFeedback combatFeedback;
    private PlayerCollector currentChopper; // Kept for legacy compatibility if other scripts check this, but largely unused

    void Awake()
    {
        combatFeedback = GetComponent<CombatFeedback>();
    }

    void Start()
    {
        currentHits = hitsToBreak;
    }

    public void TakeHit()
    {
        currentHits--;
        Debug.Log($"Chop! Hits Left: {currentHits}");

        if (combatFeedback != null)
        {
            combatFeedback.OnHit();
        }
        else 
        {
            // Fallback if no CombatFeedback component
            if (visualModel == null) visualModel = gameObject; // Ensure we have something to shake
            StartCoroutine(ShakeVisual());
        }

        if (currentHits <= 0)
        {
            Harvest();
        }
    }

    void Harvest()
    {
        Debug.Log($"Harvesting {gameObject.name}!");
        if (resourcePrefab != null)
        {
            LootSpawner.Spawn(resourcePrefab, transform.position, resourceAmount, explosionForce);
        }

        // Fix Infinite Chop: Force player to stop chopping before we destroy the trigger
        if (currentChopper != null)
        {
            currentChopper.SetChoppingState(false);
            currentChopper = null;
        }

        Destroy(gameObject);
    }

    // Fallback Legacy Wobble (Kept in case CombatFeedback isn't used)
    IEnumerator ShakeVisual()
    {
        Vector3 originalScale = visualModel.transform.localScale;
        visualModel.transform.localScale = originalScale * 1.2f;
        yield return new WaitForSeconds(0.1f);
        visualModel.transform.localScale = originalScale;
    }

    // TRIGGERS can be used solely for setting the "IsChopping" ANIMATION state on the player
    // but the actual damage comes from the axe hit.
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !other.isTrigger)
        {
            currentChopper = other.GetComponent<PlayerCollector>();
            if (currentChopper != null)
            {
                currentChopper.SetChoppingState(true);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && !other.isTrigger)
        {
            if (currentChopper != null)
            {
                currentChopper.SetChoppingState(false);
                currentChopper = null;
            }
        }
    }
}