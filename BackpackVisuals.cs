using UnityEngine;
using System.Collections.Generic;

public class BackpackVisuals : MonoBehaviour
{
    [Header("Settings")]
    public Transform backpackAnchor;
    public GameObject woodPrefab;
    public GameObject stonePrefab;
    public float stackOffset = 0.2f; // Distance between items
    public int maxVisualStack = 10;

    private List<GameObject> spawnedItems = new List<GameObject>();

    public void UpdateBackpack(int woodCount, int stoneCount)
    {
        // 1. Calculate total visuals needed
        int totalItems = woodCount + stoneCount;
        int visualsToShow = Mathf.Min(totalItems, maxVisualStack);

        // 2. Clear old items
        foreach (var item in spawnedItems) Destroy(item);
        spawnedItems.Clear();

        if (backpackAnchor == null) return;

        // 3. Spawn Items
        int currentSpawned = 0;

        // Stack Wood First
        for (int i = 0; i < woodCount; i++)
        {
            if (currentSpawned >= visualsToShow) break;
            SpawnItem(woodPrefab, currentSpawned);
            currentSpawned++;
        }

        // Stack Stone Second
        for (int i = 0; i < stoneCount; i++)
        {
            if (currentSpawned >= visualsToShow) break;
            SpawnItem(stonePrefab, currentSpawned);
            currentSpawned++;
        }
    }

    void SpawnItem(GameObject prefab, int index)
    {
        if (prefab == null) return;

        GameObject newItem = Instantiate(prefab, backpackAnchor);
        newItem.transform.localPosition = new Vector3(0, index * stackOffset, 0); 
        newItem.transform.localRotation = Quaternion.identity;
        
        // --- THE FIX ---
        // Strip the "Brain" and "Body" from the visual item
        // so the Magnet doesn't eat it.
        DisablePickupLogic(newItem);

        spawnedItems.Add(newItem);
    }

    private void DisablePickupLogic(GameObject item)
    {
        // 1. Remove the Pickup Script (The Brain)
        var pickupScript = item.GetComponent<ResourcePickup>();
        if (pickupScript != null) Destroy(pickupScript);

        // 2. Remove the Collider (The Body that Magnet sees)
        var col = item.GetComponent<Collider>();
        if (col != null) Destroy(col);

        // 3. Remove Rigidbody (The Physics)
        var rb = item.GetComponent<Rigidbody>();
        if (rb != null) Destroy(rb);
    }
}