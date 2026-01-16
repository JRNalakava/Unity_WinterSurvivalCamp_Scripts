using UnityEngine;

public class WeaponHitbox : RaycastHurtbox
{
    [Header("Weapon Settings")]
    public float weaponLength = 1.0f;

    public override Vector3 GetTipPosition()
    {
        // Simple forward projection from the transform (the handle/hand)
        return transform.position + transform.up * weaponLength;
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 tip = transform.position + transform.up * weaponLength;
        Gizmos.DrawLine(transform.position, tip);
        Gizmos.DrawWireSphere(tip, hitRadius);
    }
}
