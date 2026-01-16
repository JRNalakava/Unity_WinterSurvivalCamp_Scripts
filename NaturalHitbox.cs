using UnityEngine;

public class NaturalHitbox : RaycastHurtbox
{
    [Header("Bone References")]
    [Tooltip("The 'Handle' of the attack (e.g. Neck)")]
    public Transform startBone;
    [Tooltip("The 'Tip' of the attack (e.g. Nose/Mouth)")]
    public Transform endBone;

    public override Vector3 GetTipPosition()
    {
        if (endBone != null)
        {
            return endBone.position;
        }
        return transform.position;
    }
    
    private void OnDrawGizmosSelected()
    {
        if (startBone != null && endBone != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(startBone.position, endBone.position);
            Gizmos.DrawWireSphere(endBone.position, hitRadius);
        }
    }
}
