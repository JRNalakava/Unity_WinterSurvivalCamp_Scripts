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
    
    [Header("Detection")]
    public Vector3 detectionSize = new Vector3(4f, 2f, 4f);
    public Vector3 detectionOffset = Vector3.zero;

    private Coroutine currentRoutine;
    private Quaternion openRot;
    private Quaternion closedRot;

    void Start()
    {
        // 0. Ensure Trigger Collider Exists
        BoxCollider boxCol = GetComponent<BoxCollider>();
        if (boxCol == null)
        {
            boxCol = gameObject.AddComponent<BoxCollider>();
        }
        
        UpdateSettings();

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

    void OnValidate()
    {
        UpdateSettings();
    }

    public void UpdateSettings()
    {
        // Auto-assign references if missing
        if (pivot == null) pivot = transform;
        if (visualMesh == null)
        {
            // Try to find a child mesh that is NOT the pivot itself (if pivot is this transform)
            // Assuming Visual Mesh is a child
            if (transform.childCount > 0)
            {
                visualMesh = transform.GetChild(0); 
            }
        }

        // Configure Trigger Collider if present
        BoxCollider boxCol = GetComponent<BoxCollider>();
        if (boxCol != null)
        {
            boxCol.isTrigger = true;
            boxCol.size = detectionSize;
            boxCol.center = detectionOffset;
        }

        // 1. Calculate Rotations based on Direction
        float direction = swivelRight ? -1f : 1f;
        closedRot = Quaternion.Euler(0, 0, 0); // Assuming 0 is default
        openRot = Quaternion.Euler(0, openAngle * direction, 0);

        // 2. Apply Offset to make it hinge on the side
        if (visualMesh != null)
        {
            // Check if visualMesh is same as pivot - if so, we can't offset geometry without moving pivot
            if (visualMesh == pivot)
            {
                Debug.LogWarning("GateController: Visual Mesh is the same object as Pivot. Rotation will be centered.", this);
            }
            else
            {
                // Move mesh half-width in the opposite direction of the hinge
                float xOffset = swivelRight ? -(gateWidth / 2) : (gateWidth / 2);
                visualMesh.localPosition = new Vector3(xOffset, 0, 0);
            }
        }
        else
        {
            Debug.LogWarning("GateController: Visual Mesh not assigned! Gate will rotate in place.", this);
        }
    }

    [ContextMenu("Auto-Calculate Width")]
    public void AutoCalculateWidth()
    {
        if (visualMesh != null)
        {
            MeshFilter filter = visualMesh.GetComponent<MeshFilter>();
            if (filter != null && filter.sharedMesh != null)
            {
                // Calculate width based on mesh bounds X size and local scale
                gateWidth = filter.sharedMesh.bounds.size.x * visualMesh.localScale.x;
                UpdateSettings();
                Debug.Log($"GateController: Auto-calculated width: {gateWidth}", this);
            }
            else
            {
                Debug.LogWarning("GateController: Cannot calculate width. Visual Mesh missing MeshFilter.", this);
            }
        }
        else
        {
            Debug.LogWarning("GateController: Assign Visual Mesh first!", this);
        }
    }

    [ContextMenu("Create Trigger Collider")]
    public void CreateTriggerCollider()
    {
        BoxCollider boxCol = GetComponent<BoxCollider>();
        if (boxCol == null)
        {
            boxCol = gameObject.AddComponent<BoxCollider>();
            UpdateSettings();
            Debug.Log("GateController: Created Trigger Collider.", this);
        }
        else
        {
            UpdateSettings();
            Debug.Log("GateController: Updated existing Trigger Collider.", this);
        }
    }

    // --- API ---

    public void OpenGate(Transform opener = null)
    {
        if (isOpen) return;
        isOpen = true;

        Quaternion targetRot = openRot;

        if (opener != null)
        {
            // Determine side: Front or Back?
            // "Front" is defined as the direction of the gate's parent forward (or World Z if no parent)
            // assuming the gate is closed at local rotation 0.
            Vector3 closedForward = transform.parent != null ? transform.parent.forward : Vector3.forward;
            Vector3 dirToOpener = opener.position - transform.position;
            float dot = Vector3.Dot(closedForward, dirToOpener);

            // Default 'direction' (calculated in UpdateSettings) opens towards Positive Z (Front)
            // If player is in Front (dot > 0), we want to open to Back (Invert direction) -> Wait, user said opposite.
            // Current code was: (dot > 0) ? -baseDir : baseDir;
            // User says "it's the opposite", so let's flip it.
            float baseDir = swivelRight ? -1f : 1f; 
            float finalDirFactor = (dot > 0) ? baseDir : -baseDir;
            
            targetRot = Quaternion.Euler(0, openAngle * finalDirFactor, 0);
        }

        RotateVisuals(targetRot);

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
            OpenGate(other.transform);
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
