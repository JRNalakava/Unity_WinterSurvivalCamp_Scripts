using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Settings")]
    // 1. Set your exact desired view here. 
    // Recommended Start: X=0, Y=12, Z=-10
    public Vector3 offset = new Vector3(0, 12, -10); 
    
    // 2. How much lag? 0 = Instant (No Jitter), 0.2 = Smooth
    public float smoothTime = 0.15f; 

    private Vector3 _currentVelocity;

    void LateUpdate()
    {
        if (target == null) return;

        // Calculate the desired position based on the WORLD offset
        // We do NOT rotate with the player, keeping the camera steady "North"
        Vector3 targetPos = target.position + offset;

        // SmoothDamp is critical for removing the "Diagonal Jitter"
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref _currentVelocity, smoothTime);
    }
}