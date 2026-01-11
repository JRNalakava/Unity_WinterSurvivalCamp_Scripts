using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class GateController : MonoBehaviour
{
    [Header("References")]
    public Transform pivot;
    public Transform visualMesh; // The actual gate model
    public Collider physicsCollider; // Blocks Player
    public NavMeshObstacle navObstacle; // Blocks AI

    [Header("Settings")]
    public bool swivelRight = true; // True = Hinge on Right, False = Hinge on Left
    public float gateWidth = 4.0f;
    public float openAngle = 90f;
    public float rotateSpeed = 5f;
    public bool isOpen = false;

    private Coroutine currentRoutine;
    private Quaternion openRot;
    private Quaternion closedRot;

    void Start()
    {
        // 1. Calculate Rotations based on Direction
        float direction = swivelRight ? -1f : 1f;
        closedRot = Quaternion.Euler(0, 0, 0); // Assuming 0 is default
        openRot = Quaternion.Euler(0, openAngle * direction, 0);

        // 2. Apply Offset to make it hinge on the side
        if (visualMesh != null)
        {
            // Move mesh half-width in the opposite direction of the hinge
            float xOffset = swivelRight ? -(gateWidth / 2) : (gateWidth / 2);
            visualMesh.localPosition = new Vector3(xOffset, 0, 0);
        }

        // 3. Enforce initial state
        if (isOpen)
        {
            pivot.localRotation = openRot;
            if (physicsCollider != null) physicsCollider.enabled = false;
            if (navObstacle != null) navObstacle.enabled = false;
        }
        else
        {
            pivot.localRotation = closedRot;
            if (physicsCollider != null) physicsCollider.enabled = true;
            if (navObstacle != null) navObstacle.enabled = true;
        }
    }

    // --- API ---

    public void OpenGate()
    {
        if (isOpen) return;
        isOpen = true;

        RotateVisuals(openRot);

        if (physicsCollider != null) physicsCollider.enabled = false;
        if (navObstacle != null) navObstacle.enabled = false;
    }

    public void CloseGate()
    {
        if (!isOpen) return;
        isOpen = false;

        RotateVisuals(closedRot);

        if (physicsCollider != null) physicsCollider.enabled = true;
        if (navObstacle != null) navObstacle.enabled = true;
    }

    // --- INTERNALS ---

    void RotateVisuals(Quaternion targetRot)
    {
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(RotateSmoothly(targetRot));
    }

    IEnumerator RotateSmoothly(Quaternion targetRot)
    {
        while (Quaternion.Angle(pivot.localRotation, targetRot) > 0.1f)
        {
            // Changed from Lerp (Exponential) to RotateTowards (Linear Speed)
            // rotateSpeed is now degrees per second (e.g., 90)
            float step = rotateSpeed * 10f * Time.deltaTime; // Multiplied by 10 to make default '5' feel faster (50 deg/s)
            pivot.localRotation = Quaternion.RotateTowards(pivot.localRotation, targetRot, step);
            yield return null;
        }
        
        pivot.localRotation = targetRot;
    }

    // --- TRIGGERS ---

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OpenGate();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CloseGate();
        }
    }
}
