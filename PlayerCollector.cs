using UnityEngine;

public class PlayerCollector : MonoBehaviour
{    
    [Header("Inventory")]
    public float chopSpeed = 1.0f; 
    public float magnetRadius = 5f;
    public int woodCount = 0;
    public int stoneCount = 0;

    [Header("Components")]
    public Animator animator;
    public BackpackVisuals backpack;

    [Header("Visuals")]
    public GameObject woodPrefab;
    public GameObject stonePrefab;

    private const string CHOP_BOOL = "IsChopping"; 

    private void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    public void SetChoppingState(bool isChopping)
    {
        if (animator != null)
        {
            animator.SetBool(CHOP_BOOL, isChopping);
            
            if (isChopping)
            {
                float safeSpeed = Mathf.Max(0.1f, chopSpeed);
                animator.speed = 1.0f / safeSpeed; 
            }
            else
            {
                animator.speed = 1.0f;
            }
        }
    }

    private void Update()
    {
        // Magnet Logic
        Collider[] hits = Physics.OverlapSphere(transform.position, magnetRadius);
        foreach (var hit in hits)
        {
            ResourcePickup pickup = hit.GetComponentInParent<ResourcePickup>();
            if (pickup != null)
            {
                pickup.MagnetTo(transform);
            }
        }
    }

    public void AddResource(Harvestable.ResourceType type, int amount)
    {
        if (type == Harvestable.ResourceType.Wood)
        {
            woodCount += amount;
        }
        else if (type == Harvestable.ResourceType.Stone)
        {
            stoneCount += amount;
        }
        
        if (backpack != null) backpack.UpdateBackpack(woodCount, stoneCount);
    }

    // --- THE FIX IS HERE ---
    public GameObject RemoveWood()
    {
        if (woodCount > 0)
        {
            woodCount--;
            if (backpack != null) backpack.UpdateBackpack(woodCount, stoneCount);
            
            // Spawn the visual
            GameObject visualLog = null;
            if (woodPrefab != null)
            {
                visualLog = Instantiate(woodPrefab, transform.position + Vector3.up, Quaternion.identity);
            }
            else
            {
                visualLog = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            }

            // CRITICAL: Disable the Collider and Script so we don't pick it up again!
            DisablePickupLogic(visualLog);
            
            return visualLog;
        }
        return null;
    }

    public GameObject RemoveStone()
    {
        if (stoneCount > 0)
        {
            stoneCount--;
            if (backpack != null) backpack.UpdateBackpack(woodCount, stoneCount);
            
            GameObject visualRock = null;
            if (stonePrefab != null)
            {
                visualRock = Instantiate(stonePrefab, transform.position + Vector3.up, Quaternion.identity);
            }
            else
            {
                visualRock = GameObject.CreatePrimitive(PrimitiveType.Cube);
            }

            // CRITICAL: Disable the Collider and Script so we don't pick it up again!
            DisablePickupLogic(visualRock);

            return visualRock;
        }
        return null;
    }

    // Helper function to strip physics/logic from visual items
    private void DisablePickupLogic(GameObject item)
    {
        if (item == null) return;

        // Remove the Pickup Script
        ResourcePickup pickupScript = item.GetComponent<ResourcePickup>();
        if (pickupScript != null) Destroy(pickupScript);

        // Remove the Collider (so Magnet doesn't see it)
        Collider col = item.GetComponent<Collider>();
        if (col != null) Destroy(col);

        // Remove Rigidbody (so it doesn't fall while flying)
        Rigidbody rb = item.GetComponent<Rigidbody>();
        if (rb != null) Destroy(rb);
    }
}