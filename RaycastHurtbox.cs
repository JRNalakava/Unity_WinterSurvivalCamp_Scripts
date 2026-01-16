using UnityEngine;
using System.Collections.Generic;

public abstract class RaycastHurtbox : MonoBehaviour
{
    [Header("Settings")]
    public float damage = 10f;
    public LayerMask targetLayers;
    public GameObject hitParticles;
    [Tooltip("Radius of the sphere/ray. 0 = Raycast, >0 = SphereCast")]
    public float hitRadius = 0.1f;

    [Header("Debug")]
    public bool isAttacking;
    
    // Internal state
    private Vector3 lastTipPos;
    private HashSet<GameObject> alreadyHit = new HashSet<GameObject>();

    // Children must implement this to tell us where the "Tip" of the weapon is currently
    public abstract Vector3 GetTipPosition();

    public void StartAttack()
    {
        isAttacking = true;
        alreadyHit.Clear();
        lastTipPos = GetTipPosition(); // prevent teleport hits from idle->attack start
    }

    public void EndAttack()
    {
        isAttacking = false;
    }

    protected virtual void LateUpdate()
    {
        // Always track tip position to ensure next frame's ray is valid
        Vector3 currentTipPos = GetTipPosition();

        if (isAttacking)
        {
            DetectHit(lastTipPos, currentTipPos);
        }

        lastTipPos = currentTipPos;
    }

    private void DetectHit(Vector3 start, Vector3 end)
    {
        Vector3 direction = end - start;
        float distance = direction.magnitude;

        if (distance <= 0.001f) return;

        RaycastHit[] hits;

        if (hitRadius > 0)
        {
            // SphereCast for "Thick" detection
            hits = Physics.SphereCastAll(start, hitRadius, direction.normalized, distance, targetLayers);
        }
        else
        {
            // Simple Raycast
            hits = Physics.RaycastAll(start, direction.normalized, distance, targetLayers);
        }

        foreach (RaycastHit hit in hits)
        {
            GameObject target = hit.collider.gameObject;
            if (!alreadyHit.Contains(target))
            {
                alreadyHit.Add(target);
                ApplyDamage(hit.collider);
                
                if (hitParticles != null)
                {
                    Instantiate(hitParticles, hit.point, Quaternion.LookRotation(hit.normal));
                }
            }
        }
    }

    protected virtual void ApplyDamage(Collider col)
    {
        // Try EnemyHealth (if player is attacking)
        EnemyHealth enemy = col.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            return;
        }
        
        // Try PlayerHealth (if enemy is attacking)
        PlayerHealth player = col.GetComponent<PlayerHealth>();
        if (player != null)
        {
           player.TakeDamage(damage);
           return;
        }

        // Try generic IDamageable if you have one later
    }

    private void OnDrawGizmos()
    {
        if (isAttacking) 
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(GetTipPosition(), hitRadius);
        }
    }
}
