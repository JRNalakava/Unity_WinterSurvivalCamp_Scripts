using UnityEngine;
using System.Collections;

public class ResourcePickup : MonoBehaviour
{
    public Harvestable.ResourceType type = Harvestable.ResourceType.Wood;
    public int amount = 1;

    private Transform target;
    private bool isFlying = false;
    private bool hasBeenCollected = false;
    
    // NEW: Prevents instant magnetization
    private bool canBeMagnetized = false; 

    void Start()
    {
        // Wait 0.8 seconds before allowing the magnet to grab this
        StartCoroutine(MagnetDelay());
    }

    IEnumerator MagnetDelay()
    {
        yield return new WaitForSeconds(0.8f);
        canBeMagnetized = true;
    }

    public void MagnetTo(Transform playerTransform)
    {
        // If we are still "exploding" (delay active), ignore the magnet
        if (!canBeMagnetized) return; 
        
        if (isFlying) return; 
        
        target = playerTransform;
        isFlying = true;

        // Strip physics so it flies straight to the player
        if (TryGetComponent(out Rigidbody rb)) Destroy(rb);
        if (TryGetComponent(out Collider col)) Destroy(col);
    }

    void Update()
    {
        if (isFlying && target != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.position + Vector3.up, 15f * Time.deltaTime);

            if (Vector3.Distance(transform.position, target.position + Vector3.up) < 0.5f)
            {
                Collect();
            }
        }
    }

    void Collect()
    {
        if (hasBeenCollected) return; 
        hasBeenCollected = true;      

        PlayerCollector player = target.GetComponent<PlayerCollector>();
        if (player != null)
        {
            player.AddResource(type, amount);
        }

        Destroy(gameObject);
    }
}