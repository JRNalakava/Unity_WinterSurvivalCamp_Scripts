using UnityEngine;
using System.Collections.Generic;

public class FenceRow : MonoBehaviour
{
    [Header("Settings")]
    public GameObject fencePrefab;
    [Min(0)] public int length = 5;
    public float spacing = 4.0f;

    // Public method for BuildingZone to calculate cost
    public int GetTotalCost(int costPerSegment)
    {
        return length * costPerSegment;
    }

    // Called whenever a value is changed in the Inspector
    void OnValidate()
    {
        // We use delayCall to avoid "SendMessage cannot be called during..." warnings
        // This is Editor-only logic
#if UNITY_EDITOR
        UnityEditor.EditorApplication.delayCall += RebuildFence;
#endif
    }

    public void RebuildFence()
    {
        // Safety check: Object might have been deleted while waiting for delayCall
        if (this == null) return;
        if (fencePrefab == null) return;

        // FIX: Prevent running on Prefab Assets (Project View)
        if (!gameObject.scene.IsValid()) return;

        // 1. Clear existing children (The old fence)
        var children = new List<GameObject>();
        foreach (Transform child in transform)
        {
            children.Add(child.gameObject);
        }

        foreach (GameObject child in children)
        {
            if (Application.isPlaying)
            {
                Destroy(child);
            }
            else
            {
                DestroyImmediate(child);
            }
        }

        // 2. Spawn new fence segments
        for (int i = 0; i < length; i++)
        {
            // Calculate position
            Vector3 spawnPos = transform.position + (transform.right * (i * spacing));
            
            // Spawn as Child
            GameObject newSegment;
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                newSegment = UnityEditor.PrefabUtility.InstantiatePrefab(fencePrefab) as GameObject;
            }
            else
            {
                newSegment = Instantiate(fencePrefab);
            }
#else
            newSegment = Instantiate(fencePrefab);
#endif

            if (newSegment != null)
            {
                newSegment.transform.position = spawnPos;
                newSegment.transform.rotation = transform.rotation;
                newSegment.transform.SetParent(transform);
            }
        }
    }
}
