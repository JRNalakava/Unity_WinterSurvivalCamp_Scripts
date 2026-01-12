using UnityEngine;
using System.Collections.Generic;

public class GatedFenceRow : MonoBehaviour
{
    public enum GateType { Single, Double }

    [Header("Settings")]
    public int totalLength = 10;
    public float spacing = 4.0f;
    public GateType gateType = GateType.Single;

    [Header("Prefabs")]
    public GameObject fencePrefab;
    public GameObject singleGatePrefab;
    public GameObject doubleGatePrefab;

    // Called whenever a value is changed in the Inspector
    void OnValidate()
    {
        // Editor-only delay call to rebuild
#if UNITY_EDITOR
        UnityEditor.EditorApplication.delayCall += RebuildGatedFence;
#endif
    }

    public void RebuildGatedFence()
    {
        // Safety checks
        if (this == null) return;
        if (fencePrefab == null) return;
        if (!gameObject.scene.IsValid()) return;

        // 1. Clear existing children
        var children = new List<GameObject>();
        foreach (Transform child in transform)
        {
            children.Add(child.gameObject);
        }

        foreach (GameObject child in children)
        {
            if (Application.isPlaying) Destroy(child);
            else DestroyImmediate(child);
        }

        // 2. Calculate Segments
        int leftSideLength = 0;
        int rightSideLength = 0;
        int gateWidthSegments = 0;

        if (gateType == GateType.Single)
        {
            // Single Gate: Needs 1 segment gap.
            gateWidthSegments = 1;
            leftSideLength = (totalLength - gateWidthSegments) / 2; 
        }
        else // Double
        {
            // Double Gate: Needs 2 segment gap.
            gateWidthSegments = 2;
            leftSideLength = (totalLength - gateWidthSegments) / 2;
        }
        
        // Calculate Right side to pick up any remainder from integer division
        // This ensures Left + Gate + Right == TotalLength always.
        rightSideLength = totalLength - gateWidthSegments - leftSideLength;

        if (leftSideLength < 0) leftSideLength = 0;
        if (rightSideLength < 0) rightSideLength = 0;

        // 3. Spawn Left Fence
        CreateFenceSection("Left Fence", leftSideLength, 0);

        // 4. Spawn Gate
        GameObject gateToSpawn = (gateType == GateType.Single) ? singleGatePrefab : doubleGatePrefab;
        if (gateToSpawn != null)
        {
            GameObject gateObj;
#if UNITY_EDITOR
            if (!Application.isPlaying)
                gateObj = UnityEditor.PrefabUtility.InstantiatePrefab(gateToSpawn) as GameObject;
            else
                gateObj = Instantiate(gateToSpawn);
#else
            gateObj = Instantiate(gateToSpawn);
#endif
            if (gateObj != null)
            {
                // Usage of false for worldPositionStays ensures we keep the prefab's local transform 
                // relative to the new parent, which we immediately overwrite.
                gateObj.transform.SetParent(transform, false); 
                
                gateObj.transform.localRotation = Quaternion.identity;
                gateObj.transform.localScale = Vector3.one;
                
                // Position: End of Left Fence. 
                float gatePos = leftSideLength * spacing;
                gateObj.transform.localPosition = new Vector3(gatePos, 0, 0);
                
                Debug.Log($"GatedFenceRow: Spawned {(gateType)} at Local X: {gatePos}. World: {gateObj.transform.position}", gateObj);
            }
        }

        // 5. Spawn Right Fence
        // Starts after the gate. 
        float rightStartOffset = (leftSideLength + gateWidthSegments) * spacing;
        CreateFenceSection("Right Fence", rightSideLength, rightStartOffset);
    }

    void CreateFenceSection(string name, int length, float xStartOffset)
    {
        if (length <= 0) return;

        GameObject sectionObj = new GameObject(name);
        sectionObj.transform.SetParent(transform);
        sectionObj.transform.localPosition = new Vector3(xStartOffset, 0, 0);
        sectionObj.transform.localRotation = Quaternion.identity;

        FenceRow row = sectionObj.AddComponent<FenceRow>();
        row.fencePrefab = fencePrefab;
        row.length = length;
        row.spacing = spacing;
        
        // Trigger generic build
        row.RebuildFence();
    }
}
